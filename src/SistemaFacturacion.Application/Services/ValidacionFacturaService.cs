using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Application.DTOs;
using SistemaFacturacion.Domain.Configuration;

namespace SistemaFacturacion.Application.Services;

public class ValidacionFacturaService : IValidacionFacturaService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IComprobanteElectronicoRepository _comprobanteRepo;
    private readonly IXmlGeneratorService _xmlGenerator;
    private readonly IXmlValidationService _xmlValidator;
    private readonly SriConfiguracion _sriConfig;

    public ValidacionFacturaService(
        IFacturaRepository facturaRepo,
        IComprobanteElectronicoRepository comprobanteRepo,
        IXmlGeneratorService xmlGenerator,
        IXmlValidationService xmlValidator,
        SriConfiguracion sriConfig)
    {
        _facturaRepo = facturaRepo;
        _comprobanteRepo = comprobanteRepo;
        _xmlGenerator = xmlGenerator;
        _xmlValidator = xmlValidator;
        _sriConfig = sriConfig;
    }

    public async Task<List<FacturaValidacionDto>> GetFacturasPagadasParaValidacionAsync(CancellationToken ct = default)
    {
        var facturas = await _facturaRepo.GetFacturasPagadasAsync(includeCliente: true, ct);

        // ✅ ORDENAR POR ID DESCENDENTE ANTES DE PROCESAR
        var facturasOrdenadas = facturas.OrderByDescending(f => f.IdFactura).ToList();

        var resultado = new List<FacturaValidacionDto>();

        foreach (var factura in facturasOrdenadas) // ✅ Usar la lista ordenada
        {
            var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(factura.IdFactura, ct);

            resultado.Add(new FacturaValidacionDto
            {
                IdFactura = factura.IdFactura,
                NumeroFactura = factura.NumeroFactura,
                NombreCliente = $"{factura.Cliente?.Nombres} {factura.Cliente?.Apellidos}".Trim(),
                FechaEmision = factura.FechaEmision,
                Total = factura.Total,
                Estado = factura.Estado,
                TieneXmlGenerado = !string.IsNullOrEmpty(comprobante?.XmlGenerado),
                EstadoValidacion = comprobante?.EstadoEnvio,
                ClaveAcceso = comprobante?.ClaveAcceso
            });
        }

        return resultado;
    }


    public async Task<string> GenerarXmlFacturaAsync(int idFactura, CancellationToken ct = default)
    {
        return await _xmlGenerator.GenerarXmlFactura(idFactura, ct);
    }

    public async Task<(bool IsValid, List<string> Errors)> ValidarEstructuraXmlAsync(int idFactura, CancellationToken ct = default)
    {
        // Obtener el XML de la factura
        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);

        if (comprobante == null || string.IsNullOrEmpty(comprobante.XmlGenerado))
        {
            return (false, new List<string> { "No existe XML generado para esta factura. Debe generarlo primero." });
        }

        // Usar la ruta del XSD desde la configuración
        var xsdPath = _sriConfig.RutaXsdFactura;

        // Validar contra el XSD
        return await _xmlValidator.ValidarXmlContraXsdAsync(comprobante.XmlGenerado, xsdPath, ct);
    }
}
