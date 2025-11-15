using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class ComprobanteElectronicoRepository : IComprobanteElectronicoRepository
{
    private readonly ApplicationDbContext _db;

    public ComprobanteElectronicoRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ComprobanteElectronico?> GetByFacturaIdAsync(int idFactura, CancellationToken ct = default)
    {
        return await _db.ComprobantesElectronicos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdFactura == idFactura, ct);
    }

    public async Task<ComprobanteElectronico?> GetByClaveAccesoAsync(string claveAcceso, CancellationToken ct = default)
    {
        return await _db.ComprobantesElectronicos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClaveAcceso == claveAcceso, ct);
    }

    public async Task<ComprobanteElectronico> AddAsync(ComprobanteElectronico comprobante, CancellationToken ct = default)
    {
        _db.ComprobantesElectronicos.Add(comprobante);
        await _db.SaveChangesAsync(ct);
        return comprobante;
    }

    public async Task<ComprobanteElectronico> UpdateAsync(ComprobanteElectronico comprobante, CancellationToken ct = default)
    {
        var existente = await _db.ComprobantesElectronicos
            .FirstOrDefaultAsync(c => c.IdComprobante == comprobante.IdComprobante, ct);

        if (existente == null)
            throw new InvalidOperationException($"Comprobante con ID {comprobante.IdComprobante} no encontrado");

        // Actualizar campos
        existente.ClaveAcceso = comprobante.ClaveAcceso;
        existente.XmlGenerado = comprobante.XmlGenerado;
        existente.XmlFirmado = comprobante.XmlFirmado;
        existente.EstadoEnvio = comprobante.EstadoEnvio;
        existente.MensajeRespuesta = comprobante.MensajeRespuesta;
        existente.FechaEnvio = comprobante.FechaEnvio;
        existente.FechaAutorizacion = comprobante.FechaAutorizacion;
        existente.NumeroAutorizacion = comprobante.NumeroAutorizacion;

        await _db.SaveChangesAsync(ct);
        return existente;
    }

    public async Task<List<ComprobanteElectronico>> GetByEstadoAsync(string estado, CancellationToken ct = default)
    {
        return await _db.ComprobantesElectronicos
            .AsNoTracking()
            .Include(c => c.Factura)
            .Where(c => c.EstadoEnvio == estado)
            .OrderByDescending(c => c.IdComprobante)
            .ToListAsync(ct);
    }
}
