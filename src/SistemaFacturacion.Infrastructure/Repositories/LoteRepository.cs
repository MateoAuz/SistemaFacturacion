using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class LoteRepository : ILoteRepository
{
    private readonly ApplicationDbContext _ctx;

    public LoteRepository(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<Lote>> GetByProductoIdAsync(int productoId, CancellationToken ct = default)
    {
        return await _ctx.Lotes
            .Where(l => l.ProductoId == productoId)
            .OrderBy(l => l.FechaIngreso)
            .ToListAsync(ct);
    }

    public async Task<Lote?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _ctx.Lotes
            .FirstOrDefaultAsync(l => l.IdLote == id, ct);
    }

    public async Task<Lote> AddAsync(Lote entity, CancellationToken ct = default)
    {
        _ctx.Lotes.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Lote entity, CancellationToken ct = default)
    {
        _ctx.Entry(entity).State = EntityState.Modified;
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _ctx.Lotes.FirstOrDefaultAsync(l => l.IdLote == id, ct);
        if (entity is null) return;

        _ctx.Lotes.Remove(entity);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task<bool> ExisteNumeroLoteAsync(string numeroLote, CancellationToken ct = default)
    {
        return await _ctx.Lotes
            .AnyAsync(l => l.NumeroLote == numeroLote, ct);
    }
}