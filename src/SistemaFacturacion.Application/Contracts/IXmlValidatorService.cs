namespace SistemaFacturacion.Application.Contracts;

public interface IXmlValidatorService
{
    (bool esValido, List<string> errores) ValidarXmlContraXsd(string xmlContent, string tipoComprobante);
}
