using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly ITaxCalculator _taxCalculator;
    private readonly ApplicationDbContext _context;

    public FacturasController(
        IFacturaRepository facturaRepo,
        ITaxCalculator taxCalculator,
        ApplicationDbContext context)
    {
        _facturaRepo = facturaRepo;
        _taxCalculator = taxCalculator;
        _context = context;
    }

    // POST /api/facturas
    [HttpPost]
    public async Task<ActionResult<Factura>> Crear([FromBody] Factura factura, CancellationToken ct)
    {
        if (factura.IdCliente == 0)
            return BadRequest("Se requiere un cliente.");

        if (!factura.Detalles.Any())
            return BadRequest("La factura debe tener al menos un producto.");

        // 1. Recalcular totales en el backend (¡Importante!)
        decimal subtotal = 0;
        foreach (var detalle in factura.Detalles)
        {
            // NO calculamos TotalLinea aquí
            subtotal += (detalle.PrecioUnitario * detalle.Cantidad); // <-- CORRECCIÓN AQUÍ
        }

        var calculo = _taxCalculator.CalcularImpuestos(subtotal);
        factura.Subtotal = calculo.Subtotal;
        factura.Iva = calculo.MontoIva;
        factura.Total = calculo.Total;

        // Asignar ID de usuario (simulado) y fecha
        factura.IdUsuario = 1; // Debería venir del usuario autenticado
        factura.FechaEmision = DateTime.UtcNow;
        // Generamos un secuencial temporal único para la prueba
        factura.NumeroFactura = $"001-001-{Guid.NewGuid().ToString().Substring(0, 9)}";
        factura.Estado = "PENDIENTE";

        try
        {
            var facturaCreada = await _facturaRepo.CrearFacturaAsync(factura, ct);
            return Ok(facturaCreada);
        }
        catch (InvalidOperationException ex)
        {
            // Error de negocio (ej. Sin Stock)
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Otro error
            return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
        }
    }

    // GET /api/facturas
    [HttpGet]
public async Task<IActionResult> GetFacturas()
{
    var facturas = await _context.Facturas
        .Include(f => f.Cliente)
        .Include(f => f.Usuario)
        .Include(f => f.Detalles)
            .ThenInclude(d => d.Producto)
        .OrderByDescending(f => f.FechaEmision)
        .ToListAsync();

    var result = facturas.Select(f => new
    {
        f.IdFactura,
        f.NumeroFactura,
        f.FechaEmision,
        f.Subtotal,
        f.Iva,
        f.Total,
        Cliente = new
        {
            f.Cliente.IdCliente,
            f.Cliente.Nombres,
            f.Cliente.Apellidos,
            f.Cliente.Correo,
            f.Cliente.Direccion,
            f.Cliente.Identificacion,
            f.Cliente.Telefono
        },
        Usuario = new
        {
            f.Usuario.IdUsuario,
            f.Usuario.NombreUsuario
        },
        Detalles = f.Detalles.Select(d => new
        {
            d.Cantidad,
            d.PrecioUnitario,
            d.TotalLinea,
            NombreProducto = d.Producto.Nombre,
            Codigo = d.Producto.Codigo
        })
    });

    return Ok(result);
}


}