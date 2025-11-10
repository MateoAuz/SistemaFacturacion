using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class ProductosController : ControllerBase
{
    private readonly IProductoRepository _productoRepo;

    public ProductosController(IProductoRepository productoRepo)
    {
        _productoRepo = productoRepo;
    }

    // GET: api/productos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductos(CancellationToken ct)
    {
        // ✅ ACTUALIZADO: Incluir lotes para calcular StockTotal
        var productos = await _productoRepo.GetAllAsync(includeLotes: true, ct);
        return Ok(productos);
    }

    // GET: api/productos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id, CancellationToken ct)
    {
        // ✅ ACTUALIZADO: Incluir lotes
        var producto = await _productoRepo.GetByIdAsync(id, includeLotes: true, ct);
        if (producto == null) return NotFound();
        return producto;
    }

    // GET: api/productos/by-codigo/ABC123
    [HttpGet("by-codigo/{codigo}")]
    public async Task<ActionResult<Producto>> GetProductoByCodigo(string codigo, CancellationToken ct)
    {
        // ✅ ACTUALIZADO: Incluir lotes
        var producto = await _productoRepo.GetByCodigoAsync(codigo, includeLotes: true, ct);
        if (producto == null) return NotFound();
        return producto;
    }

    // POST: api/productos
    [HttpPost]
    public async Task<ActionResult<Producto>> PostProducto(Producto producto, CancellationToken ct)
    {
        // ✅ ELIMINADO: Validaciones de propiedades eliminadas
        // Solo validamos datos básicos
        if (string.IsNullOrEmpty(producto.Codigo) || string.IsNullOrEmpty(producto.Nombre))
            return BadRequest("Código y nombre son obligatorios");

        try
        {
            var productoGuardado = await _productoRepo.AddAsync(producto, ct);
            return CreatedAtAction(nameof(GetProducto), new { id = productoGuardado.IdProducto }, productoGuardado);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al crear producto: {ex.Message}");
        }
    }

    // PUT: api/productos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProducto(int id, Producto producto, CancellationToken ct)
    {
        if (id != producto.IdProducto) return BadRequest();

        try
        {
            await _productoRepo.UpdateAsync(producto, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al actualizar producto: {ex.Message}");
        }
    }

    // DELETE: api/productos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProducto(int id, CancellationToken ct)
    {
        try
        {
            await _productoRepo.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al eliminar producto: {ex.Message}");
        }
    }

    // PATCH: api/productos/5/activar
    [HttpPatch("{id}/activar")]
    public async Task<IActionResult> ActivarProducto(int id, CancellationToken ct)
    {
        try
        {
            var producto = await _productoRepo.GetByIdAsync(id, includeLotes: false, ct);
            if (producto == null) return NotFound();

            producto.Estado = true;
            await _productoRepo.UpdateAsync(producto, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al activar producto: {ex.Message}");
        }
    }

    // PATCH: api/productos/5/desactivar
    [HttpPatch("{id}/desactivar")]
    public async Task<IActionResult> DesactivarProducto(int id, CancellationToken ct)
    {
        try
        {
            var producto = await _productoRepo.GetByIdAsync(id, includeLotes: false, ct);
            if (producto == null) return NotFound();

            producto.Estado = false;
            await _productoRepo.UpdateAsync(producto, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al desactivar producto: {ex.Message}");
        }
    }
}