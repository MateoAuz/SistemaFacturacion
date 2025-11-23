namespace SistemaFacturacion.Application.Contracts;

public interface ISriApiService
{
    Task<(bool Success, string Message)> EnviarComprobanteAsync(string xmlFirmado, string claveAcceso, CancellationToken ct = default);
    Task<(bool Success, string Message, string? XmlAutorizado, string? NumeroAutorizacion)> ConsultarAutorizacionAsync(string claveAcceso, CancellationToken ct = default);
}
