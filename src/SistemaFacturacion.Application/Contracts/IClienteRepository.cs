using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IClienteRepository
{
    Task<Cliente> AddAsync(Cliente cliente, CancellationToken ct = default);
    Task<Cliente?> UpdateAsync(int id, Cliente cliente, CancellationToken ct = default);
    Task<bool> DeleteLogicalAsync(int id, CancellationToken ct = default);
    Task<Cliente?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Cliente?> GetByIdentificacionAsync(string identificacion, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> GetAllActivosAsync(CancellationToken ct = default);
}
