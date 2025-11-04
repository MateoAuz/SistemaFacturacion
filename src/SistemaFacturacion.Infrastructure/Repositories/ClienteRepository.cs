using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _db;

    public ClienteRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Cliente> AddAsync(Cliente cliente, CancellationToken ct = default)
    {
        // Normalizar identificacion/correo si aplica
        if (!string.IsNullOrWhiteSpace(cliente.Identificacion))
            cliente.Identificacion = cliente.Identificacion.Trim();

        // Validar duplicado por identificacion
        var exists = await _db.Clientes
            .AnyAsync(c => c.Identificacion == cliente.Identificacion, ct);

        if (exists)
            throw new InvalidOperationException("Ya existe un cliente con esa identificación.");

        cliente.Estado = true;

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(ct);
        return cliente;
    }

    public async Task<Cliente?> UpdateAsync(int id, Cliente input, CancellationToken ct = default)
    {
        var entity = await _db.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id, ct);
        if (entity is null) return null;

        // Si cambia identificacion, valida duplicado
        if (!string.IsNullOrWhiteSpace(input.Identificacion) &&
            input.Identificacion.Trim() != entity.Identificacion)
        {
            var dup = await _db.Clientes
                .AnyAsync(c => c.Identificacion == input.Identificacion!.Trim() && c.IdCliente != id, ct);
            if (dup)
                throw new InvalidOperationException("Ya existe otro cliente con esa identificación.");
            entity.Identificacion = input.Identificacion!.Trim();
        }

        entity.TipoIdentificacion = input.TipoIdentificacion;
        entity.Nombres = input.Nombres;
        entity.Apellidos = input.Apellidos;
        entity.Direccion = input.Direccion;
        entity.Telefono = input.Telefono;
        entity.Correo = input.Correo;
        // No cambiamos Estado aquí salvo que quieras exponerlo en PUT

        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteLogicalAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id, ct);
        if (entity is null) return false;

        if (!entity.Estado) return true; // ya está dado de baja
        entity.Estado = false;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<Cliente?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return _db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id, ct);
    }

    public Task<Cliente?> GetByIdentificacionAsync(string identificacion, CancellationToken ct = default)
    {
        identificacion = identificacion.Trim();
        return _db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Identificacion == identificacion, ct);
    }

    public async Task<IReadOnlyList<Cliente>> GetAllActivosAsync(CancellationToken ct = default)
    {
        return await _db.Clientes.AsNoTracking()
            .Where(c => c.Estado)
            .OrderBy(c => c.Nombres)
            .ThenBy(c => c.Apellidos)
            .ToListAsync(ct);
    }
}
