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
        try
        {
            
            var sql = @"
                SELECT u.* 
                FROM usuarios u 
                WHERE u.nombreusuario = @p0 
                AND u.clavehash = crypt(@p1, u.clavehash)
                AND u.estado = true";
            
            var usuario = await _db.Usuarios
                .FromSqlRaw(sql, nombreUsuario, password)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);
                
            return usuario;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en autenticación: {ex.Message}");
            
            
            return await ValidarCredencialesSimpleAsync(nombreUsuario, password, ct);
        }
    }

    
    private async Task<Usuario?> ValidarCredencialesSimpleAsync(string nombreUsuario, string password, CancellationToken ct = default)
    {
        var usuario = await GetByNombreUsuarioAsync(nombreUsuario, ct);
        
        if (usuario == null) return null;

        // Para desarrollo: verificación simple
        // Esto es TEMPORAL - en producción usar siempre crypt
       

        return null;
    }
}