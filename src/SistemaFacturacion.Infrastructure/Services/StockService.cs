using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Services; // <-- [CAMBIO 1] ESTA ES LA RUTA CORRECTA

public class StockService : IStockService
{
    private readonly ApplicationDbContext _db;

    public StockService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(bool, string)> ValidarStockAsync(IEnumerable<DetalleFactura> detalles, CancellationToken ct = default)
    {
        var idsProductos = detalles.Select(d => d.IdProducto).Distinct().ToList();
        
        var productosEnDb = await _db.Productos
            .Where(p => idsProductos.Contains(p.IdProducto))
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var item in detalles)
        {
            var producto = productosEnDb.FirstOrDefault(p => p.IdProducto == item.IdProducto);
            if (producto == null)
            {
                return (false, $"Producto con ID {item.IdProducto} no encontrado.");
            }

            if (producto.StockActual < item.Cantidad)
            {
                return (false, $"Stock insuficiente para '{producto.Nombre}'. Stock actual: {producto.StockActual}, Solicitado: {item.Cantidad}.");
            }
        }
        
        return (true, string.Empty);
    }
    
    public async Task DescontarStockAsync(IEnumerable<DetalleFactura> detalles, CancellationToken ct = default)
    {
        var idsProductos = detalles.Select(d => d.IdProducto).Distinct().ToList();
        
        var productosEnDb = await _db.Productos
            .Where(p => idsProductos.Contains(p.IdProducto))
            .ToListAsync(ct);

        foreach (var item in detalles)
        {
            var producto = productosEnDb.FirstOrDefault(p => p.IdProducto == item.IdProducto);
            if (producto != null)
            {
                producto.StockActual -= item.Cantidad;
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}