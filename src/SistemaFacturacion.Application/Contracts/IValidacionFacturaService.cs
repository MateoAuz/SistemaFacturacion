using SistemaFacturacion.Application.DTOs;

namespace SistemaFacturacion.Application.Contracts;

public interface IValidacionFacturaService
{
    Task<List<FacturaValidacionDto>> GetFacturasPagadasParaValidacionAsync(CancellationToken ct = default);
    Task<string> GenerarXmlFacturaAsync(int idFactura, CancellationToken ct = default);
    Task<(bool IsValid, List<string> Errors)> ValidarEstructuraXmlAsync(int idFactura, CancellationToken ct = default);
}
