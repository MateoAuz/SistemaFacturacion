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
        IRideGeneratorService rideGenerator,      // ✅ AGREGAR
        IEmailService emailService,
        ILogger<FacturacionElectronicaService> logger)              // ✅ AGREGAR

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
        // Obtener facturas PAGADAS con XML validado
        var facturas = await _facturaRepo.GetFacturasPagadasAsync(includeCliente: true, ct);
        var resultado = new List<FacturaElectronicaDto>();

        foreach (var factura in facturas)
        {
            var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(factura.IdFactura, ct);

            // Solo mostrar facturas que tienen XML generado
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

    // ✅ NUEVO MÉTODO: Para reemplazar XML cuando hubo rechazo
    public async Task<(bool Success, string Message)> ReemplazarXmlFirmadoAsync(int idFactura, string nuevoXmlFirmado, CancellationToken ct = default)
    {
        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);
        
        if (comprobante == null)
            return (false, "No se encontró el comprobante electrónico");
        
        // NO permitir reemplazar si ya está autorizado
        if (comprobante.EstadoEnvio == "AUTORIZADO")
            return (false, "El comprobante ya está autorizado, no se puede reemplazar");
        
        try
        {
            // Validar formato básico XML
            var xmlDoc = System.Xml.Linq.XDocument.Parse(nuevoXmlFirmado);
            
            // Actualizar XML firmado
            comprobante.XmlFirmado = nuevoXmlFirmado;
            comprobante.EstadoEnvio = "FIRMADO";  // Reset a firmado para poder enviar
            comprobante.FechaFirma = DateTime.UtcNow; // Limpiar errores previos
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

    public async Task<byte[]> GenerarYEnviarRideAsync(int idFactura, CancellationToken ct = default)
{
    // 1. Generar PDF (esto es lo más importante - no debe fallar)
    var pdfBytes = await _rideGenerator.GenerarRidePdfAsync(idFactura, ct);

    // 2. Obtener datos de la factura (Incluyendo Cliente)
    var factura = await _facturaRepo.GetByIdAsync(idFactura, includeCliente: true, ct: ct);

    if (factura == null)
        throw new Exception("Factura no encontrada");

    // 3. Intentar enviar por correo (en un bloque try-catch separado)
    if (factura.Cliente != null && !string.IsNullOrEmpty(factura.Cliente.Correo))
    {
        try
        {
            // Validación básica de formato de correo
            if (!factura.Cliente.Correo.Contains("@") || !factura.Cliente.Correo.Contains("."))
            {
                // Si el formato es inválido, simplemente no enviamos el correo
                // pero NO interrumpimos el flujo (el usuario sigue recibiendo su PDF)
                // Aquí podrías agregar un log si quieres:
                _logger.LogWarning($"Correo inválido para factura {idFactura}: {factura.Cliente.Correo}");
                return pdfBytes; // Retornamos el PDF sin intentar enviar
            }

            // Nombre del cliente para personalizar el correo
            string nombreCliente = factura.Cliente.Nombres ?? "Cliente";

            // Intentar enviar el correo
            await _emailService.EnviarFacturaAsync(
                emailDestino: factura.Cliente.Correo,
                nombreCliente: nombreCliente,
                pdfBytes: pdfBytes,
                numeroFactura: factura.NumeroFactura,
                ct: ct);

            // Si llegamos aquí, el correo se envió correctamente
            // Puedes agregar un log de éxito si quieres:
            _logger.LogInformation($"Correo enviado exitosamente a {factura.Cliente.Correo} para factura {idFactura}");
        }
        catch (Exception ex)
        {
            // Si falla el envío del correo (servidor caído, correo no existe, etc.)
            // capturamos el error pero NO lanzamos la excepción hacia arriba
            // para que el usuario IGUAL reciba su PDF descargado.
            
            // Aquí deberías registrar el error en un log para revisarlo después:
            _logger.LogError(ex, $"Error al enviar correo para factura {idFactura} a {factura.Cliente.Correo}");
            
            // El usuario no verá un error, simplemente su PDF se descargará
            // sin que se haya enviado el correo.
        }
    }
    // Si no hay correo o el cliente es null, simplemente no enviamos correo

    // 4. Retornar el PDF SIEMPRE (sin importar si el correo se envió o no)
    return pdfBytes;
}

}
