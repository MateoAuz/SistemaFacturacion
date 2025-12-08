using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace SistemaFacturacion.Application.Services;

public class FacturacionElectronicaService : IFacturacionElectronicaService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IComprobanteElectronicoRepository _comprobanteRepo;
    private readonly ISriApiService _sriApi;
    private readonly IRideGeneratorService _rideGenerator;
    private readonly IEmailService _emailService;
    private readonly ILogger<FacturacionElectronicaService> _logger;

    public FacturacionElectronicaService(
        IFacturaRepository facturaRepo,
        IComprobanteElectronicoRepository comprobanteRepo,
        ISriApiService sriApi,
        IRideGeneratorService rideGenerator,      
        IEmailService emailService,
        ILogger<FacturacionElectronicaService> logger)             

    {
        _facturaRepo = facturaRepo;
        _comprobanteRepo = comprobanteRepo;
        _sriApi = sriApi;
        _rideGenerator = rideGenerator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<FacturaElectronicaDto>> GetFacturasParaEnvioAsync(CancellationToken ct = default)
    {
        var facturas = await _facturaRepo.GetFacturasPagadasAsync(includeCliente: true, ct);
        var resultado = new List<FacturaElectronicaDto>();

        foreach (var factura in facturas)
        {
            var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(factura.IdFactura, ct);

            if (comprobante != null && !string.IsNullOrEmpty(comprobante.XmlGenerado))
            {
                resultado.Add(new FacturaElectronicaDto
                {
                    IdFactura = factura.IdFactura,
                    NumeroFactura = factura.NumeroFactura,
                    NombreCliente = $"{factura.Cliente?.Nombres} {factura.Cliente?.Apellidos}".Trim(),
                    FechaEmision = factura.FechaEmision,
                    Total = factura.Total,
                    ClaveAcceso = comprobante.ClaveAcceso,
                    EstadoEnvio = comprobante.EstadoEnvio,
                    TieneXmlFirmado = !string.IsNullOrEmpty(comprobante.XmlFirmado),
                    FechaEnvio = comprobante.FechaEnvio,
                    FechaAutorizacion = comprobante.FechaAutorizacion
                });
            }
        }

        return resultado.OrderByDescending(f => f.FechaEmision).ToList();
    }

    public async Task<bool> SubirXmlFirmadoAsync(int idFactura, string xmlFirmado, CancellationToken ct = default)
    {
        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);

        if (comprobante == null)
        {
            throw new InvalidOperationException("No se encontró el comprobante electrónico");
        }

        comprobante.XmlFirmado = xmlFirmado;
        comprobante.FechaFirma = DateTime.Now;
        comprobante.EstadoEnvio = "FIRMADO";

        await _comprobanteRepo.UpdateAsync(comprobante, ct);
        return true;
    }

    public async Task<(bool Success, string Message)> ReemplazarXmlFirmadoAsync(int idFactura, string nuevoXmlFirmado, CancellationToken ct = default)
    {
        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);

        if (comprobante == null)
            return (false, "No se encontró el comprobante electrónico");

        if (comprobante.EstadoEnvio == "AUTORIZADO")
            return (false, "El comprobante ya está autorizado, no se puede reemplazar");

        try
        {
            var xmlDoc = System.Xml.Linq.XDocument.Parse(nuevoXmlFirmado);

            // Actualizar XML firmado
            comprobante.XmlFirmado = nuevoXmlFirmado;
            comprobante.EstadoEnvio = "FIRMADO";  
            comprobante.FechaFirma = DateTime.UtcNow; 
            comprobante.MensajeRespuesta = null;

            await _comprobanteRepo.UpdateAsync(comprobante, ct);

            return (true, "XML firmado actualizado correctamente. Ahora puede reenviarlo.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al procesar XML: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> EnviarAlSriAsync(int idFactura, CancellationToken ct = default)
    {
        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);

        if (comprobante == null)
        {
            return (false, "No se encontró el comprobante electrónico");
        }

        if (string.IsNullOrEmpty(comprobante.XmlFirmado))
        {
            return (false, "Debe subir el XML firmado primero");
        }

        // Enviar al SRI
        var (success, message) = await _sriApi.EnviarComprobanteAsync(
            comprobante.XmlFirmado,
            comprobante.ClaveAcceso,
            ct);

        if (success)
        {
            comprobante.EstadoEnvio = "ENVIADO";
            comprobante.FechaEnvio = DateTime.UtcNow;
            await _comprobanteRepo.UpdateAsync(comprobante, ct);
        }
        else
        {
            comprobante.EstadoEnvio = "ERROR_ENVIO";
            comprobante.MensajeRespuesta = message;
            await _comprobanteRepo.UpdateAsync(comprobante, ct);
        }

        return (success, message);
    }

    public async Task<(bool Success, string Message)> ConsultarAutorizacionAsync(int idFactura, CancellationToken ct = default)
    {
        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);

        if (comprobante == null)
        {
            return (false, "No se encontró el comprobante electrónico");
        }

        var (success, message, xmlAutorizado, numeroAutorizacion) = await _sriApi.ConsultarAutorizacionAsync(
            comprobante.ClaveAcceso,
            ct);

        if (success && xmlAutorizado != null)
        {
            comprobante.EstadoEnvio = "AUTORIZADO";
            comprobante.XmlAutorizado = xmlAutorizado;
            comprobante.NumeroAutorizacion = numeroAutorizacion;
            comprobante.FechaAutorizacion = DateTime.UtcNow;
            await _comprobanteRepo.UpdateAsync(comprobante, ct);
        }
        else
        {
            comprobante.EstadoEnvio = "RECHAZADO";
            comprobante.MensajeRespuesta = message;
            await _comprobanteRepo.UpdateAsync(comprobante, ct);
        }

        return (success, message);
    }

    private string DesescaparXml(string xmlEscapado)
{
    return xmlEscapado
        .Replace("&lt;", "<")
        .Replace("&gt;", ">")
        .Replace("&amp;", "&")
        .Replace("&quot;", "\"")
        .Replace("&apos;", "'")
        .Replace("&#xD;", "")     
        .Replace("&#xA;", "\n");  
}


    public async Task<byte[]> GenerarYEnviarRideAsync(int idFactura, CancellationToken ct = default)
{
    var pdfBytes = await _rideGenerator.GenerarRidePdfAsync(idFactura, ct);

    var factura = await _facturaRepo.GetByIdAsync(idFactura, includeCliente: true, ct: ct);

    if (factura == null)
        throw new Exception("Factura no encontrada");

    var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);

    if (comprobante == null || string.IsNullOrEmpty(comprobante.XmlAutorizado))
    {
        _logger.LogWarning($"No hay XML autorizado para factura {idFactura}");
        return pdfBytes;
    }

    string xmlParaEnviar = comprobante.XmlAutorizado;
    
    if (comprobante.XmlAutorizado.Contains("<autorizacion>"))
    {
        _logger.LogInformation($"✅ XML es AUTORIZADO para factura {idFactura}");
        
        xmlParaEnviar = DesescaparXml(comprobante.XmlAutorizado);
    }
    else
    {
        _logger.LogWarning($"❌ El campo XmlAutorizado contiene XML FIRMADO para factura {idFactura}");
        return pdfBytes;
    }

    if (factura.Cliente != null && !string.IsNullOrEmpty(factura.Cliente.Correo))
    {
        try
        {
            if (!factura.Cliente.Correo.Contains("@") || !factura.Cliente.Correo.Contains("."))
            {
                _logger.LogWarning($"Correo inválido para factura {idFactura}: {factura.Cliente.Correo}");
                return pdfBytes;
            }

            string nombreCliente = $"{factura.Cliente.Nombres} {factura.Cliente.Apellidos}".Trim();
            if (string.IsNullOrEmpty(nombreCliente))
                nombreCliente = "Cliente";

            var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlParaEnviar);

            await _emailService.EnviarFacturaConXmlAsync(
                emailDestino: factura.Cliente.Correo,
                nombreCliente: nombreCliente,
                pdfBytes: pdfBytes,
                xmlBytes: xmlBytes,
                numeroFactura: factura.NumeroFactura,
                ct: ct);

            _logger.LogInformation($"✅ PDF + XML autorizado enviados a {factura.Cliente.Correo} para factura {idFactura}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar correo para factura {idFactura}");
        }
    }

    return pdfBytes;
}


}
