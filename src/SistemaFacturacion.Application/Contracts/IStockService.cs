using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IStockService
{
    Task<(bool, string)> ValidarStockAsync(IEnumerable<DetalleFactura> detalles, CancellationToken ct = default);
    Task DescontarStockAsync(IEnumerable<DetalleFactura> detalles, CancellationToken ct = default);
}