using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

[XmlRoot("factura", Namespace = "", IsNullable = false)]
public class FacturaXml
{
    [XmlAttribute("id")]
    public string Id { get; set; } = "comprobante";
    
    [XmlAttribute("version")]
    public string Version { get; set; } = "2.1.0";
    
    [XmlElement("infoTributaria")]
    public InfoTributaria InfoTributaria { get; set; } = new();
    
    [XmlElement("infoFactura")]
    public InfoFactura InfoFactura { get; set; } = new();
    
    [XmlArray("detalles")]
    [XmlArrayItem("detalle")]
    public List<DetalleXml> Detalles { get; set; } = new();
    
    [XmlArray("infoAdicional")]
    [XmlArrayItem("campoAdicional")]
    public List<CampoAdicional>? InfoAdicional { get; set; }
}
