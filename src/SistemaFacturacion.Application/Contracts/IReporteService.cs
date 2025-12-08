/* src/SistemaFacturacion.Application/Contracts/IReporteService.cs */
using SistemaFacturacion.Application.DTOs;

namespace SistemaFacturacion.Application.Contracts
{
    public interface IReporteService
    {
        // Agregamos parámetro 'agrupacion' (DIA, MES, ANIO)
        Task<List<ReporteVentasDto>> GetVentasPorPeriodoAsync(DateTime inicio, DateTime fin, string agrupacion = "DIA", CancellationToken ct = default);
        
        Task<List<ReporteProductoDto>> GetProductosMasVendidosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
        
        // Nuevo método para la auditoría de ventas
        Task<List<ReporteAuditoriaVentaDto>> GetAuditoriaVentasAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
        
        // Mantenemos el de cambios de precio por compatibilidad si se desea
        Task<List<ReporteAuditoriaDto>> GetAuditoriaPreciosAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
    }
}