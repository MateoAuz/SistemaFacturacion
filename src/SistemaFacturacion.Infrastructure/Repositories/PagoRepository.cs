using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class PagoRepository : IPagoRepository
{
    private readonly ApplicationDbContext _ctx;

    public PagoRepository(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Pago> AddAsync(Pago pago, CancellationToken ct = default)
    {
        _ctx.Pagos.Add(pago);
        await _ctx.SaveChangesAsync(ct);
        return pago;
    }

    public async Task<IReadOnlyList<Pago>> GetByFacturaIdAsync(int facturaId, CancellationToken ct = default)
    {
        return await _ctx.Pagos
            .AsNoTracking()
            .Include(p => p.Usuario)
            .Where(p => p.IdFactura == facturaId && p.Estado == "REGISTRADO")
            .OrderBy(p => p.FechaPago)
            .ToListAsync(ct);
    }
    
    public async Task<Pago?> GetByIdAsync(int idPago, CancellationToken ct = default)
    {
        return await _ctx.Pagos.FirstOrDefaultAsync(p => p.IdPago == idPago, ct);
    }

    public async Task<bool> UpdateEstadoAsync(int idPago, string nuevoEstado, CancellationToken ct = default)
    {
        var pago = await _ctx.Pagos.FirstOrDefaultAsync(p => p.IdPago == idPago, ct);
        if (pago is null) return false;

        pago.Estado = nuevoEstado;
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}