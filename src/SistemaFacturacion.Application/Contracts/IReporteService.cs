using SistemaFacturacion.Application.DTOs;

namespace SistemaFacturacion.Application.Contracts
{
    public interface IReporteService
    {
        Task<List<ReporteVentasDto>> GetVentasPorPeriodoAsync(DateTime inicio, DateTime fin, string agrupacion = "DIA", CancellationToken ct = default);
        Task<List<ReporteProductoDto>> GetProductosMasVendidosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
        Task<List<ReporteAuditoriaVentaDto>> GetAuditoriaVentasAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
    }
}