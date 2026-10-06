using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturacionElectronicaController : ControllerBase
{
    private readonly IFacturacionElectronicaService _facturacionService;

    public FacturacionElectronicaController(IFacturacionElectronicaService facturacionService)
    {
        _facturacionService = facturacionService;
    }

    [HttpGet("facturas")]
    public async Task<IActionResult> GetFacturas(CancellationToken ct)
    {
        try
        {
            var facturas = await _facturacionService.GetFacturasParaEnvioAsync(ct);
            return Ok(facturas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("subir-xml-firmado/{idFactura}")]
    public async Task<IActionResult> SubirXmlFirmado(int idFactura, [FromBody] SubirXmlRequest request, CancellationToken ct)
    {
        try
        {
            await _facturacionService.SubirXmlFirmadoAsync(idFactura, request.XmlFirmado, ct);
            return Ok(new { success = true, message = "XML firmado subido correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("enviar-sri/{idFactura}")]
    public async Task<IActionResult> EnviarAlSri(int idFactura, CancellationToken ct)
    {
        try
        {
            var (success, message) = await _facturacionService.EnviarAlSriAsync(idFactura, ct);
            return Ok(new { success, message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("consultar-autorizacion/{idFactura}")]
    public async Task<IActionResult> ConsultarAutorizacion(int idFactura, CancellationToken ct)
    {
        try
        {
            var (success, message) = await _facturacionService.ConsultarAutorizacionAsync(idFactura, ct);
            return Ok(new { success, message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public record SubirXmlRequest(string XmlFirmado);
