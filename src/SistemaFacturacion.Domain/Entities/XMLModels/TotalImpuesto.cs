using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;


public class TotalImpuesto
{
    [XmlElement("codigo")]
    public string Codigo { get; set; } = string.Empty; // 2 = IVA

    [XmlElement("codigoPorcentaje")]
    public string CodigoPorcentaje { get; set; } = string.Empty; // 2 = 12%, 0 = 0%, 6 = No Objeto de Impuesto

    [XmlElement("baseImponible")]
    public string BaseImponible { get; set; } = string.Empty;

    [XmlElement("tarifa")]
    public string? Tarifa { get; set; }

    [XmlElement("valor")]
    public string Valor { get; set; } = string.Empty;

    

}
