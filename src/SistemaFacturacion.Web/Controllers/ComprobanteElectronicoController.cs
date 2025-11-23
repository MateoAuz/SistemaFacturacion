using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;

namespace SistemaFacturacion.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprobanteElectronicoController : ControllerBase
{
    private readonly IXmlGeneratorService _xmlGeneratorService;
    private readonly IComprobanteElectronicoRepository _comprobanteRepository;
    private readonly IXmlValidationService _xmlValidationService; // ✅ AGREGAR ESTA LÍNEA

    public ComprobanteElectronicoController(
        IXmlGeneratorService xmlGeneratorService,
        IComprobanteElectronicoRepository comprobanteRepository,
        IXmlValidationService xmlValidationService) // ✅ AGREGAR ESTE PARÁMETRO
    {
        _xmlGeneratorService = xmlGeneratorService;
        _comprobanteRepository = comprobanteRepository;
        _xmlValidationService = xmlValidationService; // ✅ AGREGAR ESTA ASIGNACIÓN
    }

    /// <summary>
    /// Genera el XML de una factura
    /// </summary>
    [HttpPost("generar-xml/{idFactura}")]
    public async Task<IActionResult> GenerarXml(int idFactura, CancellationToken ct)
    {
        try
        {
            var xmlGenerado = await _xmlGeneratorService.GenerarXmlFactura(idFactura, ct);
            
            return Ok(new
            {
                success = true,
                message = "XML generado exitosamente",
                idFactura = idFactura,
                xml = xmlGenerado
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Obtiene el comprobante electrónico de una factura
    /// </summary>
    [HttpGet("factura/{idFactura}")]
    public async Task<IActionResult> ObtenerComprobante(int idFactura, CancellationToken ct)
    {
        try
        {
            var comprobante = await _comprobanteRepository.GetByFacturaIdAsync(idFactura, ct);
            
            if (comprobante == null)
                return NotFound(new { message = "No se encontró comprobante para esta factura" });
            
            return Ok(new
            {
                success = true,
                comprobante = new
                {
                    comprobante.IdComprobante,
                    comprobante.IdFactura,
                    comprobante.ClaveAcceso,
                    comprobante.EstadoEnvio,
                    comprobante.FechaEnvio,
                    XmlGenerado = comprobante.XmlGenerado
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Descarga el XML de una factura
    /// </summary>
    [HttpGet("descargar-xml/{idFactura}")]
    public async Task<IActionResult> DescargarXml(int idFactura, CancellationToken ct)
    {
        try
        {
            var comprobante = await _comprobanteRepository.GetByFacturaIdAsync(idFactura, ct);
            
            if (comprobante == null || string.IsNullOrEmpty(comprobante.XmlGenerado))
                return NotFound(new { message = "XML no encontrado" });
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(comprobante.XmlGenerado);
            return File(bytes, "application/xml", $"factura_{idFactura}.xml");
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
/// Valida el XML de un comprobante contra el esquema XSD del SRI
/// </summary>
[HttpPost("validar-xml/{idFactura}")]
public async Task<IActionResult> ValidarXml(int idFactura, CancellationToken ct)
{
    try
    {
        // Obtener el comprobante
        var comprobante = await _comprobanteRepository.GetByFacturaIdAsync(idFactura, ct);
        
        if (comprobante == null)
            return NotFound(new { success = false, message = "Comprobante no encontrado" });
        
        if (string.IsNullOrEmpty(comprobante.XmlGenerado))
            return BadRequest(new { success = false, message = "El comprobante no tiene XML generado" });
        
        // Validar contra XSD del SRI
        var xsdPath = "Resources/XSD/factura_V2.1.0.xsd";
        var (esValido, mensajes) = await _xmlValidationService.ValidarXmlContraXsdAsync(
            comprobante.XmlGenerado, 
            xsdPath, 
            ct);
        
        return Ok(new
        {
            success = esValido,
            message = esValido ? "✅ XML válido según esquema XSD del SRI" : "❌ XML con errores",
            idFactura = idFactura,
            claveAcceso = comprobante.ClaveAcceso,
            errores = mensajes
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

}
