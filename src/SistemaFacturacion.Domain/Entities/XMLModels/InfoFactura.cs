using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

[XmlRoot("infoFactura")]
public class InfoFactura
{
    [XmlElement("fechaEmision")]
    public string FechaEmision { get; set; } = string.Empty;
    
    [XmlElement("dirEstablecimiento")]
    public string DirEstablecimiento { get; set; } = string.Empty;
    
    [XmlElement("contribuyenteEspecial")]
    public string? ContribuyenteEspecial { get; set; }
    
    [XmlElement("obligadoContabilidad")]
    public string? ObligadoContabilidad { get; set; }
    
    [XmlElement("tipoIdentificacionComprador")]
    public string TipoIdentificacionComprador { get; set; } = string.Empty;
    
    [XmlElement("guiaRemision")]
    public string? GuiaRemision { get; set; }
    
    [XmlElement("razonSocialComprador")]
    public string RazonSocialComprador { get; set; } = string.Empty;
    
    [XmlElement("identificacionComprador")]
    public string IdentificacionComprador { get; set; } = string.Empty;
    
    // ✅ ELIMINAR direccionComprador - NO está en este XSD
    // [XmlElement("direccionComprador")]
    // public string? DireccionComprador { get; set; }
    
    [XmlElement("totalSinImpuestos")]
    public string TotalSinImpuestos { get; set; } = string.Empty;
    
    [XmlElement("totalDescuento")]
    public string TotalDescuento { get; set; } = "0.00";
    
    [XmlArray("totalConImpuestos")]
    [XmlArrayItem("totalImpuesto")]
    public List<TotalImpuesto> TotalConImpuestos { get; set; } = new();
    
    [XmlElement("propina")]
    public string Propina { get; set; } = "0.00";
    
    [XmlElement("importeTotal")]
    public string ImporteTotal { get; set; } = string.Empty;
    
    [XmlElement("moneda")]
    public string? Moneda { get; set; }
}
