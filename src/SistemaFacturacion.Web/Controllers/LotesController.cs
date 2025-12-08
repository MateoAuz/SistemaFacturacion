using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class LotesController : ControllerBase
{
    private readonly ILoteRepository _loteRepo;

    public LotesController(ILoteRepository loteRepo)
    {
        _loteRepo = loteRepo;
    }

    [HttpGet("by-producto/{productoId}")]
    public async Task<ActionResult<IEnumerable<Lote>>> GetLotesByProducto(int productoId, CancellationToken ct)
    {
        var lotes = await _loteRepo.GetByProductoIdAsync(productoId, ct);
        return Ok(lotes);
    }

[HttpPost]
public async Task<ActionResult<Lote>> PostLote([FromBody] LoteCreateDto loteDto, CancellationToken ct)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var fechaIngreso = NormalizarDateTime(loteDto.FechaIngreso);
        var fechaExpiracion = loteDto.FechaExpiracion.HasValue 
            ? NormalizarDateTime(loteDto.FechaExpiracion.Value) 
            : (DateTime?)null;

        var lote = new Lote
        {
            NumeroLote = loteDto.NumeroLote,
            ProductoId = loteDto.ProductoId,
            FechaIngreso = fechaIngreso,
            FechaExpiracion = fechaExpiracion,
            CantidadInicial = loteDto.CantidadInicial,
            CantidadActual = loteDto.CantidadActual,
            PrecioCompra = loteDto.PrecioCompra
        };

        var loteGuardado = await _loteRepo.AddAsync(lote, ct);
        return CreatedAtAction(nameof(GetLote), new { id = loteGuardado.IdLote }, loteGuardado);
    }
    catch (Exception ex)
    {
        return BadRequest($"Error al crear lote: {ex.Message}");
    }
}

private DateTime NormalizarDateTime(DateTime fecha)
{
    if (fecha.Kind == DateTimeKind.Unspecified)
    {
        return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
    }
    return fecha.ToUniversalTime();
}

    [HttpGet("{id}")]
    public async Task<ActionResult<Lote>> GetLote(int id, CancellationToken ct)
    {
        var lote = await _loteRepo.GetByIdAsync(id, ct);
        if (lote == null) return NotFound();
        return lote;
    }

    [HttpPut("{id}")]
public async Task<IActionResult> PutLote(int id, [FromBody] LoteCreateDto loteDto, CancellationToken ct)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var loteExistente = await _loteRepo.GetByIdAsync(id, ct);
        if (loteExistente == null) return NotFound();

        var fechaIngreso = loteDto.FechaIngreso;
        if (fechaIngreso.Kind == DateTimeKind.Unspecified)
        {
            fechaIngreso = DateTime.SpecifyKind(fechaIngreso, DateTimeKind.Utc);
        }

        DateTime? fechaExpiracion = null;
        if (loteDto.FechaExpiracion.HasValue)
        {
            fechaExpiracion = loteDto.FechaExpiracion.Value;
            if (fechaExpiracion.Value.Kind == DateTimeKind.Unspecified)
            {
                fechaExpiracion = DateTime.SpecifyKind(fechaExpiracion.Value, DateTimeKind.Utc);
            }
        }

        // Actualizar propiedades
        loteExistente.NumeroLote = loteDto.NumeroLote;
        loteExistente.FechaIngreso = fechaIngreso;
        loteExistente.FechaExpiracion = fechaExpiracion;
        loteExistente.CantidadInicial = loteDto.CantidadInicial;
        loteExistente.CantidadActual = loteDto.CantidadActual;
        loteExistente.PrecioCompra = loteDto.PrecioCompra;

        await _loteRepo.UpdateAsync(loteExistente, ct);
        return NoContent();
    }
    catch (Exception ex)
    {
        return BadRequest($"Error al actualizar lote: {ex.Message}");
    }
}

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLote(int id, CancellationToken ct)
    {
        try
        {
            await _loteRepo.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al eliminar lote: {ex.Message}");
        }
    }
}