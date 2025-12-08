/* src/SistemaFacturacion.Infrastructure/Services/ReporteService.cs */
using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Application.DTOs;
using SistemaFacturacion.Infrastructure.Persistence;
using System.Globalization;

namespace SistemaFacturacion.Infrastructure.Services;

public class ReporteService : IReporteService
{
    private readonly ApplicationDbContext _db;

    public ReporteService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ReporteVentasDto>> GetVentasPorPeriodoAsync(DateTime inicio, DateTime fin, string agrupacion = "DIA", CancellationToken ct = default)
    {
        var fechaInicio = DateTime.SpecifyKind(inicio.Date, DateTimeKind.Utc);
        var fechaFin = DateTime.SpecifyKind(fin.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var facturas = await _db.Facturas
            .AsNoTracking()
            .Where(f => f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin && f.Estado != "ANULADA")
            .ToListAsync(ct);

        // Agrupación en memoria para mayor flexibilidad con formatos de fecha
        IEnumerable<IGrouping<string, SistemaFacturacion.Domain.Entities.Factura>> grupos;

        if (agrupacion == "MES")
        {
            grupos = facturas.GroupBy(f => f.FechaEmision.ToString("yyyy-MM"));
        }
        else if (agrupacion == "ANIO")
        {
            grupos = facturas.GroupBy(f => f.FechaEmision.ToString("yyyy"));
        }
        else // DIA
        {
            grupos = facturas.GroupBy(f => f.FechaEmision.ToString("yyyy-MM-dd"));
        }

        return grupos.Select(g => new ReporteVentasDto
        {
            Periodo = g.Key,
            CantidadFacturas = g.Count(),
            Subtotal = g.Sum(x => x.Subtotal),
            Iva = g.Sum(x => x.Iva),
            Total = g.Sum(x => x.Total)
        })
        .OrderBy(x => x.Periodo)
        .ToList();
    }

    public async Task<List<ReporteProductoDto>> GetProductosMasVendidosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default)
    {
        var fechaInicio = DateTime.SpecifyKind(inicio.Date, DateTimeKind.Utc);
        var fechaFin = DateTime.SpecifyKind(fin.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

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
                        TotalGenerado = g.Sum(x => x.TotalLinea),
                        // Precio Promedio = Total / Cantidad
                        PrecioPromedio = g.Sum(x => x.Cantidad) > 0 
                            ? g.Sum(x => x.TotalLinea) / g.Sum(x => x.Cantidad) 
                            : 0
                    };

        return await query
            .OrderByDescending(x => x.CantidadVendida)
            .Take(10) 
            .ToListAsync(ct);
    }

    public async Task<List<ReporteAuditoriaVentaDto>> GetAuditoriaVentasAsync(DateTime inicio, DateTime fin, CancellationToken ct = default)
    {
        var fechaInicio = DateTime.SpecifyKind(inicio.Date, DateTimeKind.Utc);
        var fechaFin = DateTime.SpecifyKind(fin.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Comparamos Precio Venta Real (Detalle) vs Precio Oficial Actual (Producto)
        // Nota: Si el precio oficial cambió, esto mostrará la diferencia contra el actual.
        var query = from d in _db.DetallesFactura
                    join f in _db.Facturas on d.IdFactura equals f.IdFactura
                    join p in _db.Productos on d.IdProducto equals p.IdProducto
                    join c in _db.Clientes on f.IdCliente equals c.IdCliente
                    join u in _db.Usuarios on f.IdUsuario equals u.IdUsuario
                    where f.FechaEmision >= fechaInicio && 
                          f.FechaEmision <= fechaFin && 
                          f.Estado != "ANULADA"
                    select new ReporteAuditoriaVentaDto
                    {
                        Fecha = f.FechaEmision,
                        NumeroFactura = f.NumeroFactura,
                        Cliente = c.Nombres + " " + (c.Apellidos ?? ""),
                        Producto = p.Nombre,
                        PrecioLista = p.PrecioVenta,
                        PrecioVendido = d.PrecioUnitario,
                        Descuento = p.PrecioVenta - d.PrecioUnitario,
                        Usuario = u.NombreUsuario
                    };

        return await query.OrderByDescending(x => x.Fecha).ToListAsync(ct);
    }

    public async Task<List<ReporteAuditoriaDto>> GetAuditoriaPreciosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default)
    {
        var fechaInicio = DateTime.SpecifyKind(inicio.Date, DateTimeKind.Utc);
        var fechaFin = DateTime.SpecifyKind(fin.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        return await _db.HistorialPrecios
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
                Accion = "Cambio de Catálogo",
                Detalle = $"Cambio de ${h.PrecioAnterior:N2} a ${h.PrecioNuevo:N2}. Motivo: {h.Motivo}"
            })
            .ToListAsync(ct);
    }
}