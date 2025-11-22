using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IStockService _stockService;

    public FacturaRepository(ApplicationDbContext db, IStockService stockService)
    {
        _db = db;
        _stockService = stockService;
    }

    public async Task<Factura> CrearFacturaAsync(Factura factura, CancellationToken ct = default)
    {
        // Iniciar una transacción
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Validar Stock (doble chequeo)
            var (esValido, error) = await _stockService.ValidarStockAsync(factura.Detalles, ct);
            if (!esValido)
            {
                throw new InvalidOperationException($"Error de stock: {error}");
            }

            // 2. Guardar la cabecera de la factura
            _db.Facturas.Add(factura);
            await _db.SaveChangesAsync(ct);

            // 3. Descontar el Stock
            await _stockService.DescontarStockAsync(factura.Detalles, ct);

            // 4. Confirmar la transacción
            await tx.CommitAsync(ct);

            return factura;
        }
        catch (Exception)
        {
            // Si algo falla (guardar factura o descontar stock), revierte todo.
            await tx.RollbackAsync(ct);
            throw; // Relanza la excepción
        }
    }
    public async Task<Factura?> GetByIdAsync(int id, bool includePagos = false, bool includeDetalles = false, bool includeCliente = false, CancellationToken ct = default)
    {
        var query = _db.Facturas.AsQueryable();

        if (includePagos)
            query = query.Include(f => f.Pagos.Where(p => p.Estado == "REGISTRADO")).ThenInclude(p => p.Usuario);
        if (includeDetalles)
            query = query.Include(f => f.Detalles).ThenInclude(d => d.Producto);
        if (includeCliente)
            query = query.Include(f => f.Cliente);

        return await query.FirstOrDefaultAsync(f => f.IdFactura == id, ct);
    }

    // NUEVO: Actualizar estado y saldo pendiente de una factura
    public async Task UpdateEstadoAndSaldoAsync(int idFactura, string nuevoEstado, decimal nuevoSaldoPendiente, CancellationToken ct = default)
    {
        var factura = await _db.Facturas.FirstOrDefaultAsync(f => f.IdFactura == idFactura, ct);
        if (factura is null) throw new InvalidOperationException($"Factura ID {idFactura} no encontrada.");

        factura.Estado = nuevoEstado;
        factura.SaldoPendiente = nuevoSaldoPendiente;

        await _db.SaveChangesAsync(ct);
    }

    // NUEVO: Obtener todas las facturas con opciones de filtrado
    public async Task<IReadOnlyList<Factura>> GetAllAsync(string? estado = null, bool includeCliente = true, bool includeDetalles = false, CancellationToken ct = default)
    {
        var query = _db.Facturas.AsNoTracking();

        if (estado != null && estado != "TODAS")
        {
            query = query.Where(f => f.Estado == estado);
        }

        if (includeCliente) query = query.Include(f => f.Cliente);
        if (includeDetalles) query = query.Include(f => f.Detalles).ThenInclude(d => d.Producto);

        return await query.OrderByDescending(f => f.FechaEmision).ToListAsync(ct);
    }

    public async Task<List<Factura>> GetFacturasPagadasAsync(bool includeCliente = true, CancellationToken ct = default)
    {
        var query = _db.Facturas
            .AsNoTracking()
            .Where(f => f.Estado == "PAGADA");

        if (includeCliente)
        {
            query = query.Include(f => f.Cliente);
        }

        return await query
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync(ct);
    }

}