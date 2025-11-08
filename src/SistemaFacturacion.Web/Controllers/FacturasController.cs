using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly ITaxCalculator _taxCalculator;

    public FacturasController(IFacturaRepository facturaRepo, ITaxCalculator taxCalculator)
    {
        _facturaRepo = facturaRepo;
        _taxCalculator = taxCalculator;
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
}