using SistemaFacturacion.Application.DTOs;

namespace SistemaFacturacion.Application.Contracts
{
    public interface IReporteService
    {
        Task<List<ReporteVentasDto>> GetVentasPorPeriodoAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
        Task<List<ReporteProductoDto>> GetProductosMasVendidosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
        Task<List<ReporteAuditoriaDto>> GetAuditoriaPreciosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
    }
}