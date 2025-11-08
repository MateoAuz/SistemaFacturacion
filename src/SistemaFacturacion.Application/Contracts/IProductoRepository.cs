using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllAsync(CancellationToken ct = default);
    Task<Producto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken ct = default);

    Task<Producto> AddAsync(Producto entity, CancellationToken ct = default);
    Task UpdateAsync(Producto entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<int> GetTotalStockAsync(CancellationToken ct);
}
