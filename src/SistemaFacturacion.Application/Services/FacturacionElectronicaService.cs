using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Application.DTOs;

namespace SistemaFacturacion.Application.Services;

public class FacturacionElectronicaService : IFacturacionElectronicaService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IComprobanteElectronicoRepository _comprobanteRepo;
    private readonly ISriApiService _sriApi;

    public FacturacionElectronicaService(
        IFacturaRepository facturaRepo,
        IComprobanteElectronicoRepository comprobanteRepo,
        ISriApiService sriApi)
    {
        _facturaRepo = facturaRepo;
        _comprobanteRepo = comprobanteRepo;
        _sriApi = sriApi;
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

        var (success, message, xmlAutorizado) = await _sriApi.ConsultarAutorizacionAsync(
            comprobante.ClaveAcceso, 
            ct);

        if (success && xmlAutorizado != null)
        {
            comprobante.EstadoEnvio = "AUTORIZADO";
            comprobante.XmlAutorizado = xmlAutorizado;
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
}
