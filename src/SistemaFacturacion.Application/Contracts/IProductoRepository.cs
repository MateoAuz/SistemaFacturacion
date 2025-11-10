using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllAsync(bool includeLotes = false, CancellationToken ct = default);
    Task<Producto?> GetByIdAsync(int id, bool includeLotes = false, CancellationToken ct = default);
    Task<Producto?> GetByCodigoAsync(string codigo, bool includeLotes = false, CancellationToken ct = default);

    Task<Producto> AddAsync(Producto entity, CancellationToken ct = default);
    Task UpdateAsync(Producto entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    
    // ✅ ACTUALIZAR: Ahora el stock se calcula desde lotes
    Task<int> GetTotalStockAsync(CancellationToken ct = default);
}