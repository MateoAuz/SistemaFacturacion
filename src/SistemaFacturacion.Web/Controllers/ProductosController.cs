using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoRepository _repo;

    public ProductosController(IProductoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> Get(CancellationToken ct)
    {
        var items = await _repo.GetAllAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetById(int id, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-codigo/{codigo}")]
    public async Task<ActionResult<Producto>> GetByCodigo(string codigo, CancellationToken ct)
    {
        var item = await _repo.GetByCodigoAsync(codigo, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Post([FromBody] Producto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo) || string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("Código y Nombre son obligatorios.");

        var exists = await _repo.GetByCodigoAsync(dto.Codigo, ct);
        if (exists != null)
            return Conflict($"Ya existe un producto con código {dto.Codigo}.");

        var created = await _repo.AddAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdProducto }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Producto dto, CancellationToken ct)
    {
        if (id != dto.IdProducto) return BadRequest("Id de ruta y cuerpo no coinciden.");

        var current = await _repo.GetByIdAsync(id, ct);
        if (current is null) return NotFound();

        current.Codigo = dto.Codigo;
        current.Nombre = dto.Nombre;
        current.Categoria = dto.Categoria;
        current.PrecioUnitario = dto.PrecioUnitario;
        current.PrecioVenta = dto.PrecioVenta;
        current.StockActual = dto.StockActual;
        current.FechaExpiracion = dto.FechaExpiracion;
        current.Estado = dto.Estado;

        await _repo.UpdateAsync(current, ct);
        return NoContent();
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var current = await _repo.GetByIdAsync(id, ct);
        if (current is null) return NotFound();

        await _repo.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("count-stock")]
    public async Task<ActionResult<int>> GetTotalStock(CancellationToken ct)
    {
        var productos = await _repo.GetAllAsync(ct);
        var total = productos.Where(p => p.Estado).Sum(p => p.StockActual);
        return Ok(total);
    }

}
