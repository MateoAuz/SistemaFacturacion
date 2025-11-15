using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

public class ImpuestoDetalle
{
    [XmlElement("codigo")]
    public string Codigo { get; set; } = "2"; // 2 = IVA
    
    [XmlElement("codigoPorcentaje")]
    public string CodigoPorcentaje { get; set; } = string.Empty; // 2 = 12%
    
    [XmlElement("tarifa")]
    public string Tarifa { get; set; } = string.Empty; // 12
    
    [XmlElement("baseImponible")]
    public string BaseImponible { get; set; } = string.Empty;
    
    [XmlElement("valor")]
    public string Valor { get; set; } = string.Empty;
}
