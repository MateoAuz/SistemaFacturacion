using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

[XmlRoot("infoTributaria")]
public class InfoTributaria
{
    [XmlElement("ambiente")]
    public string Ambiente { get; set; } = string.Empty; // "1" pruebas, "2" producción
    
    [XmlElement("tipoEmision")]
    public string TipoEmision { get; set; } = "1"; // 1 = Normal
    
    [XmlElement("razonSocial")]
    public string RazonSocial { get; set; } = string.Empty;
    
    [XmlElement("nombreComercial")]
    public string NombreComercial { get; set; } = string.Empty;
    
    [XmlElement("ruc")]
    public string Ruc { get; set; } = string.Empty;
    
    [XmlElement("claveAcceso")]
    public string ClaveAcceso { get; set; } = string.Empty;
    
    [XmlElement("codDoc")]
    public string CodDoc { get; set; } = "01"; // 01 = Factura
    
    [XmlElement("estab")]
    public string Estab { get; set; } = string.Empty; // 001
    
    [XmlElement("ptoEmi")]
    public string PtoEmi { get; set; } = string.Empty; // 001
    
    [XmlElement("secuencial")]
    public string Secuencial { get; set; } = string.Empty; // 000000001
    
    [XmlElement("dirMatriz")]
    public string DirMatriz { get; set; } = string.Empty;
}
