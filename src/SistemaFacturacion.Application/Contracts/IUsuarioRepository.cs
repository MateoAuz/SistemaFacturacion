using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken ct = default);
    Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string password, CancellationToken ct = default);
}
