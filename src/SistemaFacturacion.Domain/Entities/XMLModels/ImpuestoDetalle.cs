using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

public class ImpuestoDetalle
{
    [XmlElement("codigo")]
    public string Codigo { get; set; } = "2"; 
    
    [XmlElement("codigoPorcentaje")]
    public string CodigoPorcentaje { get; set; } = string.Empty; 
    
    [XmlElement("tarifa")]
    public string Tarifa { get; set; } = string.Empty; 
    
    [XmlElement("baseImponible")]
    public string BaseImponible { get; set; } = string.Empty;
    
    [XmlElement("valor")]
    public string Valor { get; set; } = string.Empty;
}
