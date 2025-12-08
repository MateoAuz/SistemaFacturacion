using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;


public class TotalImpuesto
{
    [XmlElement("codigo")]
    public string Codigo { get; set; } = string.Empty; 

    [XmlElement("codigoPorcentaje")]
    public string CodigoPorcentaje { get; set; } = string.Empty;

    [XmlElement("baseImponible")]
    public string BaseImponible { get; set; } = string.Empty;

    [XmlElement("tarifa")]
    public string? Tarifa { get; set; }

    [XmlElement("valor")]
    public string Valor { get; set; } = string.Empty;

    

}
