using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly ApplicationDbContext _db;

    public ConfiguracionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<ConfiguracionEmpresa?> GetConfiguracionAsync(CancellationToken ct = default)
    {
        return _db.ConfiguracionEmpresa.AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<ConfiguracionEmpresa> SaveConfiguracionAsync(ConfiguracionEmpresa config, CancellationToken ct = default)
    {
        var existente = await _db.ConfiguracionEmpresa.FirstOrDefaultAsync(ct);

        if (existente == null)
        {
            _db.ConfiguracionEmpresa.Add(config);
        }
        else
        {
            existente.RazonSocial = config.RazonSocial;
            existente.NombreComercial = config.NombreComercial;
            existente.Ruc = config.Ruc;
            existente.DireccionMatriz = config.DireccionMatriz;
            existente.Establecimiento = config.Establecimiento;
            existente.PuntoEmision = config.PuntoEmision;
            existente.Ambiente = config.Ambiente;
            existente.RutaCertificado = config.RutaCertificado;
            existente.ClaveCertificado = config.ClaveCertificado;
            existente.CorreoEmpresa = config.CorreoEmpresa;
            existente.ObligadoContabilidad = config.ObligadoContabilidad;
        }

        await _db.SaveChangesAsync(ct);
        return existente ?? config;
    }
}