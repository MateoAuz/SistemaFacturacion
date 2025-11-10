using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface ILoteRepository
{
    Task<IEnumerable<Lote>> GetByProductoIdAsync(int productoId, CancellationToken ct = default);
    Task<Lote?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Lote> AddAsync(Lote entity, CancellationToken ct = default);
    Task UpdateAsync(Lote entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteNumeroLoteAsync(string numeroLote, CancellationToken ct = default);
}