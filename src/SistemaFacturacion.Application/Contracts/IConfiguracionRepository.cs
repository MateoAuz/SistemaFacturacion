using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IConfiguracionRepository
{
    Task<ConfiguracionEmpresa?> GetConfiguracionAsync(CancellationToken ct = default);
    Task<ConfiguracionEmpresa> SaveConfiguracionAsync(ConfiguracionEmpresa config, CancellationToken ct = default);
}