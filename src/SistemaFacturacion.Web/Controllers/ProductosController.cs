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

        // ⚙️ Forzar fecha a UTC si tiene valor
        if (dto.FechaExpiracion.HasValue)
            dto.FechaExpiracion = DateTime.SpecifyKind(dto.FechaExpiracion.Value, DateTimeKind.Utc);

        var created = await _repo.AddAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdProducto }, created);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Producto dto, CancellationToken ct)
    {
        if (dto.IdProducto == 0)
            dto.IdProducto = id;
        else if (id != dto.IdProducto)
            return BadRequest("Id de ruta y cuerpo no coinciden.");

        var current = await _repo.GetByIdAsync(id, ct);
        if (current is null)
            return NotFound();

        // ⚙️ Copiar datos actualizados
        current.Codigo = dto.Codigo;
        current.Nombre = dto.Nombre;
        current.Categoria = dto.Categoria;
        current.PrecioUnitario = dto.PrecioUnitario;
        current.PrecioVenta = dto.PrecioVenta;
        current.StockActual = dto.StockActual;
        current.Estado = dto.Estado;

        // ⚙️ Forzar fecha a UTC si tiene valor
        if (dto.FechaExpiracion.HasValue)
            current.FechaExpiracion = DateTime.SpecifyKind(dto.FechaExpiracion.Value, DateTimeKind.Utc);
        else
            current.FechaExpiracion = null;

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

    [HttpPatch("{id:int}/desactivar")]
    public async Task<IActionResult> DesactivarProducto(int id, CancellationToken ct)
    {
        var producto = await _repo.GetByIdAsync(id, ct);
        if (producto is null)
            return NotFound();

        // ⚙️ Forzar la fecha a UTC si tiene valor
        if (producto.FechaExpiracion.HasValue)
            producto.FechaExpiracion = DateTime.SpecifyKind(producto.FechaExpiracion.Value, DateTimeKind.Utc);

        producto.Estado = false;
        await _repo.UpdateAsync(producto, ct);

        return Ok(producto);
    }

    [HttpPatch("{id:int}/activar")]
    public async Task<IActionResult> ActivarProducto(int id, CancellationToken ct)
    {
        var producto = await _repo.GetByIdAsync(id, ct);
        if (producto is null)
            return NotFound();

        // ⚙️ Igual corrección: forzar la fecha a UTC
        if (producto.FechaExpiracion.HasValue)
            producto.FechaExpiracion = DateTime.SpecifyKind(producto.FechaExpiracion.Value, DateTimeKind.Utc);

        producto.Estado = true;
        await _repo.UpdateAsync(producto, ct);

        return Ok(producto);
    }



}
