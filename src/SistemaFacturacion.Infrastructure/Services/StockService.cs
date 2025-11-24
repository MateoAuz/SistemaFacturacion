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

    // ✅ ACTUALIZADO: Método para reducir stock usando FEFO con Corrección de Fechas UTC
    private async Task ReducirStockFifoAsync(int productoId, int cantidad, CancellationToken ct = default)
    {
        // 1. Obtener todos los lotes con stock positivo de la BD
        var lotesDb = await _context.Lotes
            .Where(l => l.ProductoId == productoId && l.CantidadActual > 0)
            .ToListAsync(ct);

        // 2. Ordenar en MEMORIA (FEFO)
        var lotesOrdenados = lotesDb
            .OrderBy(l => l.FechaExpiracion ?? DateTime.MaxValue) // Nulls al final
            .ThenBy(l => l.FechaIngreso)
            .ToList();

        var cantidadPendiente = cantidad;

        foreach (var lote in lotesOrdenados)
        {
            if (cantidadPendiente <= 0) break;

            // Tomar lo que se pueda de este lote
            var cantidadAReducir = Math.Min(lote.CantidadActual, cantidadPendiente);
            
            lote.CantidadActual -= cantidadAReducir;
            cantidadPendiente -= cantidadAReducir;
            
            // 🚑 FIX CRÍTICO: Asegurar que las fechas sean UTC antes de guardar
            // Esto evita el error "Cannot write DateTime with Kind=Unspecified"
            lote.FechaIngreso = AsegurarUtc(lote.FechaIngreso);
            if (lote.FechaExpiracion.HasValue)
            {
                lote.FechaExpiracion = AsegurarUtc(lote.FechaExpiracion.Value);
            }

            _context.Entry(lote).State = EntityState.Modified;
        }

        if (cantidadPendiente > 0)
        {
            throw new InvalidOperationException($"Stock insuficiente para el producto ID {productoId}. Faltan {cantidadPendiente} unidades.");
        }

        await _context.SaveChangesAsync(ct);
    }

    // Helper para normalizar fechas a UTC
    private DateTime AsegurarUtc(DateTime fecha)
    {
        if (fecha.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
        
        if (fecha.Kind == DateTimeKind.Local)
            return fecha.ToUniversalTime();
            
        return fecha;
    }

    public async Task<int> ObtenerStockDisponibleAsync(int productoId, CancellationToken ct = default)
    {
        return await _context.Lotes
            .Where(l => l.ProductoId == productoId && l.CantidadActual > 0)
            .SumAsync(l => l.CantidadActual, ct);
    }
}