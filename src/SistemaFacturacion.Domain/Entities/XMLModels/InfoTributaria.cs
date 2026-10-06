using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

[XmlRoot("infoTributaria")]
public class InfoTributaria
{
    [XmlElement("ambiente")]
    public string Ambiente { get; set; } = string.Empty; 
    
    [XmlElement("tipoEmision")]
    public string TipoEmision { get; set; } = "1"; 
    
    [XmlElement("razonSocial")]
    public string RazonSocial { get; set; } = string.Empty;
    
    [XmlElement("nombreComercial")]
    public string NombreComercial { get; set; } = string.Empty;
    
    [XmlElement("ruc")]
    public string Ruc { get; set; } = string.Empty;
    
    [XmlElement("claveAcceso")]
    public string ClaveAcceso { get; set; } = string.Empty;
    
    [XmlElement("codDoc")]
    public string CodDoc { get; set; } = "01"; 
    
    [XmlElement("estab")]
    public string Estab { get; set; } = string.Empty; 
    
    [XmlElement("ptoEmi")]
    public string PtoEmi { get; set; } = string.Empty; 
    
    [XmlElement("secuencial")]
    public string Secuencial { get; set; } = string.Empty; 
    
    [XmlElement("dirMatriz")]
    public string DirMatriz { get; set; } = string.Empty;
}
