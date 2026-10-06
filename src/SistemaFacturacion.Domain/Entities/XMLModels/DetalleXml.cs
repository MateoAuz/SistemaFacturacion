using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

public class DetalleXml
{
    [XmlElement("codigoPrincipal")]
    public string CodigoPrincipal { get; set; } = string.Empty;
    
    [XmlElement("descripcion")]
    public string Descripcion { get; set; } = string.Empty;
    
    [XmlElement("cantidad")]
    public string Cantidad { get; set; } = string.Empty;
    
    [XmlElement("precioUnitario")]
    public string PrecioUnitario { get; set; } = string.Empty;
    
    [XmlElement("descuento")]
    public string Descuento { get; set; } = "0.00";
    
    [XmlElement("precioTotalSinImpuesto")]
    public string PrecioTotalSinImpuesto { get; set; } = string.Empty;
    
    [XmlArray("impuestos")]
    [XmlArrayItem("impuesto")]
    public List<ImpuestoDetalle> Impuestos { get; set; } = new();
}
