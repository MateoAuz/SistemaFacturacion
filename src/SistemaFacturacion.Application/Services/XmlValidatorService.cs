using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using SistemaFacturacion.Application.Contracts;

namespace SistemaFacturacion.Application.Services;

public class XmlValidatorService : IXmlValidatorService
{
    private readonly List<string> _erroresValidacion = new();

    public (bool esValido, List<string> errores) ValidarXmlContraXsd(string xmlContent, string tipoComprobante)
    {
        _erroresValidacion.Clear();

        try
        {
            // Buscar el archivo XSD en la ruta base de la aplicación
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string xsdPath = Path.Combine(baseDirectory, "XSD", "factura_V2.1.0.xsd");

            // Si no está en la carpeta de salida, buscar en la raíz del proyecto
            if (!File.Exists(xsdPath))
            {
                // Intentar 2 niveles arriba (típico en Debug/Release)
                string projectPath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", ".."));
                xsdPath = Path.Combine(projectPath, "XSD", "factura_V2.1.0.xsd");
            }

            if (!File.Exists(xsdPath))
            {
                _erroresValidacion.Add($"❌ Archivo XSD no encontrado");
                _erroresValidacion.Add($"📁 Buscado en: {xsdPath}");
                _erroresValidacion.Add($"📁 Directorio base: {baseDirectory}");
                return (false, new List<string>(_erroresValidacion));
            }

            // Crear el esquema XSD
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(null, xsdPath);

            // Cargar el documento XML
            XDocument xmlDoc = XDocument.Parse(xmlContent);

            // Validar contra el esquema
            bool esValido = true;
            xmlDoc.Validate(schemas, (sender, args) =>
            {
                esValido = false;
                string nivel = args.Severity == XmlSeverityType.Error ? "❌ ERROR" : "⚠️ WARNING";
                _erroresValidacion.Add($"{nivel}: {args.Message}");
            });

            if (esValido)
            {
                _erroresValidacion.Add("✅ El XML cumple con el esquema XSD del SRI");
            }

            return (esValido, new List<string>(_erroresValidacion));
        }
        catch (XmlException ex)
        {
            _erroresValidacion.Add($"❌ Error de formato XML: {ex.Message}");
            if (ex.LineNumber > 0)
            {
                _erroresValidacion.Add($"   Línea: {ex.LineNumber}, Posición: {ex.LinePosition}");
            }
            return (false, new List<string>(_erroresValidacion));
        }
        catch (Exception ex)
        {
            _erroresValidacion.Add($"❌ Error general: {ex.Message}");
            return (false, new List<string>(_erroresValidacion));
        }
    }
}
