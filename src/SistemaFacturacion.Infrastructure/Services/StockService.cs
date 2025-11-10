using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Services;

public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;

    public StockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool, string)> ValidarStockAsync(IEnumerable<DetalleFactura> detalles, CancellationToken ct = default)
    {
        foreach (var detalle in detalles)
        {
            // ✅ ACTUALIZADO: Calcular stock desde lotes
            var stockDisponible = await _context.Lotes
                .Where(l => l.ProductoId == detalle.IdProducto && l.CantidadActual > 0)
                .SumAsync(l => l.CantidadActual, ct);

            if (stockDisponible < detalle.Cantidad)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.IdProducto == detalle.IdProducto, ct);
                
                var nombreProducto = producto?.Nombre ?? "Producto desconocido";
                return (false, $"Stock insuficiente para '{nombreProducto}'. Disponible: {stockDisponible}, Solicitado: {detalle.Cantidad}");
            }
        }
        
        return (true, string.Empty);
    }

    public async Task DescontarStockAsync(IEnumerable<DetalleFactura> detalles, CancellationToken ct = default)
    {
        foreach (var detalle in detalles)
        {
            await ReducirStockFifoAsync(detalle.IdProducto, detalle.Cantidad, ct);
        }
    }

    // ✅ NUEVO: Método para reducir stock usando FIFO
    private async Task ReducirStockFifoAsync(int productoId, int cantidad, CancellationToken ct = default)
    {
        var lotes = await _context.Lotes
            .Where(l => l.ProductoId == productoId && l.CantidadActual > 0)
            .OrderBy(l => l.FechaIngreso) // FIFO: primero los más antiguos
            .ToListAsync(ct);

        var cantidadPendiente = cantidad;

        foreach (var lote in lotes)
        {
            if (cantidadPendiente <= 0) break;

            var cantidadAReducir = Math.Min(lote.CantidadActual, cantidadPendiente);
            lote.CantidadActual -= cantidadAReducir;
            cantidadPendiente -= cantidadAReducir;
        }

        if (cantidadPendiente > 0)
        {
            throw new InvalidOperationException($"Stock insuficiente para el producto ID {productoId}. Faltan {cantidadPendiente} unidades.");
        }

        await _context.SaveChangesAsync(ct);
    }

    // ✅ NUEVO: Método auxiliar para obtener stock disponible
    public async Task<int> ObtenerStockDisponibleAsync(int productoId, CancellationToken ct = default)
    {
        return await _context.Lotes
            .Where(l => l.ProductoId == productoId && l.CantidadActual > 0)
            .SumAsync(l => l.CantidadActual, ct);
    }
}