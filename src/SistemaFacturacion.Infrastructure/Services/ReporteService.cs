using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Application.DTOs;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Services;

public class ReporteService : IReporteService
{
    private readonly ApplicationDbContext _db;

    public ReporteService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ReporteVentasDto>> GetVentasPorPeriodoAsync(DateTime inicio, DateTime fin, CancellationToken ct = default)
    {
        // 1. Aseguramos rango de fechas completo
        // CORRECCIÓN: Al usar .Date, forzamos de nuevo a UTC inmediatamente
        var fechaInicio = DateTime.SpecifyKind(inicio.Date, DateTimeKind.Utc);
        var fechaFin = DateTime.SpecifyKind(fin.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var data = await _db.Facturas
            .AsNoTracking()
            .Where(f => f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin && f.Estado != "ANULADA")
            .GroupBy(f => f.FechaEmision.Date) // Nota: GroupBy en SQL suele ignorar el Kind, aquí está bien
            .Select(g => new ReporteVentasDto
            {
                Fecha = g.Key,
                CantidadFacturas = g.Count(),
                Subtotal = g.Sum(x => x.Subtotal),
                Iva = g.Sum(x => x.Iva),
                Total = g.Sum(x => x.Total)
            })
            .OrderBy(x => x.Fecha)
            .ToListAsync(ct);

        return data;
    }

    public async Task<List<ReporteProductoDto>> GetProductosMasVendidosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default)
    {
        // CORRECCIÓN: Aplicamos la misma lógica UTC aquí
        var fechaInicio = DateTime.SpecifyKind(inicio.Date, DateTimeKind.Utc);
        var fechaFin = DateTime.SpecifyKind(fin.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Unimos Detalles -> Factura para filtrar por fecha y estado
        var query = from d in _db.DetallesFactura
                    join f in _db.Facturas on d.IdFactura equals f.IdFactura
                    join p in _db.Productos on d.IdProducto equals p.IdProducto
                    where f.FechaEmision >= fechaInicio && 
                          f.FechaEmision <= fechaFin && 
                          f.Estado != "ANULADA"
                    group d by new { p.IdProducto, p.Codigo, p.Nombre, p.Categoria } into g
                    select new ReporteProductoDto
                    {
                        Codigo = g.Key.Codigo,
                        Nombre = g.Key.Nombre,
                        Categoria = g.Key.Categoria ?? "Sin Categoría",
                        CantidadVendida = g.Sum(x => x.Cantidad),
                        TotalGenerado = g.Sum(x => x.TotalLinea)
                    };

        return await query
            .OrderByDescending(x => x.CantidadVendida)
            .Take(10) // Top 10 según PDF CA-11.3
            .ToListAsync(ct);
    }

    public async Task<List<ReporteAuditoriaDto>> GetAuditoriaPreciosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default)
    {
        // CORRECCIÓN: Aplicamos la misma lógica UTC aquí también
        var fechaInicio = DateTime.SpecifyKind(inicio.Date, DateTimeKind.Utc);
        var fechaFin = DateTime.SpecifyKind(fin.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var historial = await _db.HistorialPrecios
            .AsNoTracking()
            .Include(h => h.Producto)
            .Include(h => h.Usuario)
            .Where(h => h.FechaCambio >= fechaInicio && h.FechaCambio <= fechaFin)
            .OrderByDescending(h => h.FechaCambio)
            .Select(h => new ReporteAuditoriaDto
            {
                Id = h.IdHistorial,
                Fecha = h.FechaCambio,
                Usuario = h.Usuario != null ? h.Usuario.NombreUsuario : "Sistema",
                Producto = h.Producto != null ? h.Producto.Nombre : "Desconocido",
                Accion = "Cambio de Precio",
                Detalle = $"Cambio de ${h.PrecioAnterior:N2} a ${h.PrecioNuevo:N2}. Motivo: {h.Motivo}"
            })
            .ToListAsync(ct);

        return historial;
    }
}