using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] 
public class FacturasController : ControllerBase
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly ITaxCalculator _taxCalculator;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguracionRepository _configuracionRepo; // ✅ AGREGAR

    public FacturasController(
        IFacturaRepository facturaRepo,
        ITaxCalculator taxCalculator,
        ApplicationDbContext context,
        IConfiguracionRepository configuracionRepo) // ✅ AGREGAR
    {
        _facturaRepo = facturaRepo;
        _taxCalculator = taxCalculator;
        _context = context;
        _configuracionRepo = configuracionRepo; // ✅ AGREGAR
    }

    // POST /api/facturas
    [HttpPost]
    public async Task<ActionResult<Factura>> Crear([FromBody] Factura factura, CancellationToken ct)
    {
        if (factura.IdCliente == 0)
            return BadRequest("Se requiere un cliente.");

        if (!factura.Detalles.Any())
            return BadRequest("La factura debe tener al menos un producto.");

        decimal subtotal = 0;
        foreach (var detalle in factura.Detalles)
        {
            subtotal += (detalle.PrecioUnitario * detalle.Cantidad);
        }

        var calculo = _taxCalculator.CalcularImpuestos(subtotal);
        factura.Subtotal = calculo.Subtotal;
        factura.Iva = calculo.MontoIva;
        factura.Total = calculo.Total;

        if (factura.IdUsuario == 0) 
        {
            factura.IdUsuario = 1; 
        }
        factura.FechaEmision = DateTime.UtcNow.AddHours(-5);

        // ✅ OBTENER CONFIGURACIÓN
        var config = await _configuracionRepo.GetConfiguracionAsync(ct);
        
        if (config == null)
        {
            return BadRequest("No se ha configurado la información de la empresa. Configure el establecimiento y punto de emisión.");
        }

        // ✅ VALIDAR QUE EXISTAN LOS VALORES
        if (string.IsNullOrWhiteSpace(config.Establecimiento) || string.IsNullOrWhiteSpace(config.PuntoEmision))
        {
            return BadRequest("Debe configurar el establecimiento y punto de emisión en la configuración del sistema.");
        }

        // ✅ USAR VALORES DE CONFIGURACIÓN
        var consecutivo = await _context.Facturas.CountAsync(ct) + 1;
        factura.NumeroFactura = $"{config.Establecimiento}-{config.PuntoEmision}-{consecutivo:000000000}";
        factura.Estado = "PENDIENTE";
        factura.SaldoPendiente = factura.Total;

        try
        {
            var facturaCreada = await _facturaRepo.CrearFacturaAsync(factura, ct);
            return Ok(facturaCreada);
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

    // GET /api/facturas
    [HttpGet]
    public async Task<IActionResult> GetFacturas([FromQuery] string? estado = null) 
    {
        try
        {
            var facturas = await _facturaRepo.GetAllAsync(
                estado: estado,
                includeCliente: true,
                includeDetalles: true);

            if (!facturas.Any())
            {
                return Ok(new List<object>());
            }

            var facturasOrdenadas = facturas
                .OrderByDescending(f => f.IdFactura)
                .ToList();

            var result = facturasOrdenadas.Select(f => new
            {
                f.IdFactura,
                f.NumeroFactura,
                f.FechaEmision,
                f.Subtotal,
                f.Iva,
                f.Total,
                f.Estado,
                f.SaldoPendiente,
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
            Console.WriteLine($"Error al obtener facturas: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                details = ex.Message
            });
        }
    }
}
