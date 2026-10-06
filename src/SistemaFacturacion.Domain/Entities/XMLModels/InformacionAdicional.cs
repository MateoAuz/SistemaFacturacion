using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

public class CampoAdicional
{
    [XmlAttribute("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [XmlText]
    public string Valor { get; set; } = string.Empty;
}
