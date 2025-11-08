using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly ApplicationDbContext _ctx;

    public ProductoRepository(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(ct);

    public async Task<Producto?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Productos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProducto == id, ct);

    public async Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken ct = default)
        => await _ctx.Productos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Codigo == codigo, ct);

    public async Task<Producto> AddAsync(Producto entity, CancellationToken ct = default)
    {
        _ctx.Productos.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Producto entity, CancellationToken ct = default)
{
    _ctx.Entry(entity).State = EntityState.Modified;
    await _ctx.SaveChangesAsync(ct);
}


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _ctx.Productos.FirstOrDefaultAsync(p => p.IdProducto == id, ct);
        if (entity is null) return;

        _ctx.Productos.Remove(entity);
        await _ctx.SaveChangesAsync(ct);
    }

    // ✅ MÉTODO NUEVO: Obtener total de stock de productos activos
    public async Task<int> GetTotalStockAsync(CancellationToken ct = default)
    {
        return await _ctx.Productos
            .Where(p => p.Estado)
            .SumAsync(p => p.StockActual, ct);
    }
}
