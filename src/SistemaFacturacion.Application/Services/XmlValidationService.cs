using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using SistemaFacturacion.Application.Contracts;

namespace SistemaFacturacion.Application.Services;

public class XmlValidationService : IXmlValidationService
{
    private readonly IComprobanteElectronicoRepository _comprobanteRepo;
    private readonly IConfiguracionRepository _configuracionRepo;
    
    public XmlValidationService(
        IComprobanteElectronicoRepository comprobanteRepo,
        IConfiguracionRepository configuracionRepo)
    {
        _comprobanteRepo = comprobanteRepo;
        _configuracionRepo = configuracionRepo;
    }

    public async Task<(bool IsValid, List<string> Errors)> ValidarXmlContraXsdAsync(
        string xmlContent, 
        string xsdPath, 
        CancellationToken ct = default)
    {
        var errors = new List<string>();
        
        try
        {
            // Verificar que el archivo XSD existe
            if (!File.Exists(xsdPath))
            {
                errors.Add($"No se encontró el archivo XSD en la ruta: {xsdPath}");
                return (false, errors);
            }

            // Cargar el esquema XSD
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", xsdPath);

            // Parsear el XML
            XDocument xmlDoc = XDocument.Parse(xmlContent);

            // Validar el XML contra el XSD
            xmlDoc.Validate(schemas, (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    errors.Add($"ERROR: {e.Message}");
                }
                else if (e.Severity == XmlSeverityType.Warning)
                {
                    errors.Add($"ADVERTENCIA: {e.Message}");
                }
            });

            return (errors.Count == 0, errors);
        }
        catch (XmlSchemaValidationException ex)
        {
            errors.Add($"Error de validación de esquema: {ex.Message}");
            return (false, errors);
        }
        catch (XmlException ex)
        {
            errors.Add($"Error al parsear XML: {ex.Message}");
            return (false, errors);
        }
        catch (Exception ex)
        {
            errors.Add($"Error inesperado: {ex.Message}");
            return (false, errors);
        }
    }

    public async Task<string> DescargarXmlFacturaAsync(int idFactura, CancellationToken ct = default)
    {
        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);
        
        if (comprobante == null)
        {
            throw new InvalidOperationException("No se encontró el comprobante electrónico para esta factura.");
        }

        if (string.IsNullOrEmpty(comprobante.XmlGenerado))
        {
            throw new InvalidOperationException("Esta factura no tiene XML generado.");
        }

        return comprobante.XmlGenerado;
    }
}
