using SistemaFacturacion.Application.DTOs;

namespace SistemaFacturacion.Application.Contracts;

public interface IFacturacionElectronicaService
{
    Task<List<FacturaElectronicaDto>> GetFacturasParaEnvioAsync(CancellationToken ct = default);
    Task<bool> SubirXmlFirmadoAsync(int idFactura, string xmlFirmado, CancellationToken ct = default);
    Task<(bool Success, string Message)> EnviarAlSriAsync(int idFactura, CancellationToken ct = default);
    Task<(bool Success, string Message)> ConsultarAutorizacionAsync(int idFactura, CancellationToken ct = default);
    Task<byte[]> GenerarYEnviarRideAsync(int idFactura, CancellationToken ct = default);
}
