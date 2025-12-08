/* src/SistemaFacturacion.Web/Controllers/ReportesController.cs */
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Application.DTOs;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    [HttpGet("ventas")]
    public async Task<ActionResult<List<ReporteVentasDto>>> GetVentas(
        [FromQuery] DateTime inicio, 
        [FromQuery] DateTime fin,
        [FromQuery] string agrupacion,
        CancellationToken ct)
    {
        return Ok(await _reporteService.GetVentasPorPeriodoAsync(inicio, fin, agrupacion ?? "DIA", ct));
    }

    [HttpGet("productos-top")]
    public async Task<ActionResult<List<ReporteProductoDto>>> GetProductosTop(
        [FromQuery] DateTime inicio, 
        [FromQuery] DateTime fin, 
        CancellationToken ct)
    {
        return Ok(await _reporteService.GetProductosMasVendidosAsync(inicio, fin, ct));
    }

    [HttpGet("auditoria-ventas")]
    public async Task<ActionResult<List<ReporteAuditoriaVentaDto>>> GetAuditoriaVentas(
        [FromQuery] DateTime inicio, 
        [FromQuery] DateTime fin, 
        CancellationToken ct)
    {
        return Ok(await _reporteService.GetAuditoriaVentasAsync(inicio, fin, ct));
    }
}