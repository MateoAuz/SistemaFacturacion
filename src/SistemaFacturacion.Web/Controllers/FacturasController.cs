using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Permitir acceso sin autenticación
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

        // 1. Recalcular totales en el backend
        decimal subtotal = 0;
        foreach (var detalle in factura.Detalles)
        {
            subtotal += (detalle.PrecioUnitario * detalle.Cantidad);
        }

        var calculo = _taxCalculator.CalcularImpuestos(subtotal);
        factura.Subtotal = calculo.Subtotal;
        factura.Iva = calculo.MontoIva;
        factura.Total = calculo.Total;

        // Asignar fecha y generar número de factura
        factura.IdUsuario = 1; // Valor temporal
        factura.FechaEmision = DateTime.UtcNow;
        
        // Generar número de factura secuencial
        var consecutivo = await _context.Facturas.CountAsync() + 1;
        factura.NumeroFactura = $"001-003-{consecutivo:000000000}";
        factura.Estado = "PENDIENTE";
        factura.SaldoPendiente = factura.Total;

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
    public async Task<IActionResult> GetFacturas([FromQuery] string? estado = null) // MODIFICADO
    {
        try
        {
            var facturas = await _facturaRepo.GetAllAsync(
                estado: estado, // Pasa el filtro de estado al repositorio
                includeCliente: true,
                includeDetalles: true);

            if (!facturas.Any())
            {
                return Ok(new List<object>()); 
            }

            var result = facturas.Select(f => new
            {
                f.IdFactura,
                f.NumeroFactura,
                f.FechaEmision,
                f.Subtotal,
                f.Iva,
                f.Total,
                f.Estado, 
                f.SaldoPendiente, // <<-- AÑADIDO
                Cliente = f.Cliente == null ? null : new
                {
                    f.Cliente.IdCliente,
                    f.Cliente.Nombres,
                    f.Cliente.Apellidos,
                    f.Cliente.Correo,
                    f.Cliente.Direccion,
                    f.Cliente.Identificacion,
                    f.Cliente.Telefono
                },
                Detalles = f.Detalles.Select(d => new
                {
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.TotalLinea,
                    NombreProducto = d.Producto?.Nombre ?? "Producto no disponible",
                    Codigo = d.Producto?.Codigo ?? "N/A"
                })
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log del error
            Console.WriteLine($"Error al obtener facturas: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            return StatusCode(500, new { 
                error = "Error interno del servidor",
                details = ex.Message
            });
        }
    }
}