using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

public class FormaPago
{
    [XmlElement("formaPago")]
    public string FormaPagoCode { get; set; } = "01"; // 01 = Sin utilización del sistema financiero
    
    [XmlElement("total")]
    public string Total { get; set; } = string.Empty;
    
    [XmlElement("plazo")]
    public string? Plazo { get; set; }
    
    [XmlElement("unidadTiempo")]
    public string? UnidadTiempo { get; set; }
}
