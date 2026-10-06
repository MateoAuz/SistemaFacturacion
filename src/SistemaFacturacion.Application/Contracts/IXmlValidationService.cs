namespace SistemaFacturacion.Application.Contracts;

public interface IXmlValidationService
{
    Task<(bool IsValid, List<string> Errors)> ValidarXmlContraXsdAsync(
        string xmlContent, 
        string xsdPath, 
        CancellationToken ct = default);
    
    Task<string> DescargarXmlFacturaAsync(int idFactura, CancellationToken ct = default);
}
