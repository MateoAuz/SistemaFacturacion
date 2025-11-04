using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _db;

    public UsuarioRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken ct = default)
    {
        return _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Estado, ct);
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string password, CancellationToken ct = default)
    {
        var usuario = await GetByNombreUsuarioAsync(nombreUsuario, ct);
        if (usuario == null) return null;

        // Validar con pgcrypto (si usaste crypt en PostgreSQL)
        var sql = "SELECT password = crypt(@p0, password) AS valido FROM usuarios WHERE nombreusuario = @p1";
        var resultado = await _db.Database.SqlQueryRaw<bool>(sql, password, nombreUsuario).FirstOrDefaultAsync(ct);

        return resultado ? usuario : null;
    }
}
