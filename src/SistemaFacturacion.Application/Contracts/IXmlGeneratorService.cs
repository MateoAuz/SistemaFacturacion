namespace SistemaFacturacion.Application.Contracts;

public interface IXmlGeneratorService
{
    Task<string> GenerarXmlFactura(int idFactura, CancellationToken ct = default);
    string SerializarAXml<T>(T objeto);
}
