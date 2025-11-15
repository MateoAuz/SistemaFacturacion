using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] 
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;
    private readonly IPagoRepository _pagoRepo; 

    public PagosController(IPagoService pagoService, IPagoRepository pagoRepo)
    {
        _pagoService = pagoService;
        _pagoRepo = pagoRepo;
    }

    public record PagoRequest(
        [Required] int IdFactura, 
        [Required] decimal Monto, 
        [Required] string MetodoPago,
        int IdUsuario = 1 
    );
    
    // POST: api/pagos
    [HttpPost]
    public async Task<IActionResult> PostPago([FromBody] PagoRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var pago = await _pagoService.RegistrarPagoAsync(
                request.IdFactura, 
                request.Monto, 
                request.MetodoPago, 
                request.IdUsuario, 
                ct);
            
            return CreatedAtAction(nameof(GetPagosPorFactura), new { idFactura = pago.IdFactura }, pago);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
        }
    }
    
    // GET: api/pagos/by-factura/5 (CA-06.6)
    [HttpGet("by-factura/{idFactura:int}")]
    public async Task<ActionResult<IEnumerable<Pago>>> GetPagosPorFactura(int idFactura, CancellationToken ct)
    {
        var pagos = await _pagoRepo.GetByFacturaIdAsync(idFactura, ct);
        return Ok(pagos);
    }
    
    // PATCH: api/pagos/5/anular (CA-06.7)
    [HttpPatch("{idPago:int}/anular")]
    public async Task<IActionResult> AnularPago(int idPago, CancellationToken ct)
    {
        int idUsuarioAnulacion = 1; 
        
        try
        {
            await _pagoService.AnularPagoAsync(idPago, idUsuarioAnulacion, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
        }
    }
}