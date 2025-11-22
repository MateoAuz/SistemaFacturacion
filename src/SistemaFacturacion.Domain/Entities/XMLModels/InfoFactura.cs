using System.Xml.Serialization;

namespace SistemaFacturacion.Domain.Entities.XmlModels;

[XmlRoot("infoFactura")]
public class InfoFactura
{
    // 1. fechaEmision
    [XmlElement("fechaEmision")]
    public string FechaEmision { get; set; } = string.Empty;
    
    // 2. dirEstablecimiento (opcional)
    [XmlElement("dirEstablecimiento")]
    public string? DirEstablecimiento { get; set; }
    
    // 3. contribuyenteEspecial (opcional)
    [XmlElement("contribuyenteEspecial")]
    public string? ContribuyenteEspecial { get; set; }
    
    // 4. obligadoContabilidad (opcional)
    [XmlElement("obligadoContabilidad")]
    public string? ObligadoContabilidad { get; set; }
    
    // 5-12. Campos de comercio exterior (opcionales)
    [XmlElement("comercioExterior")]
    public string? ComercioExterior { get; set; }
    
    [XmlElement("incoTermFactura")]
    public string? IncoTermFactura { get; set; }
    
    [XmlElement("lugarIncoTerm")]
    public string? LugarIncoTerm { get; set; }
    
    [XmlElement("paisOrigen")]
    public string? PaisOrigen { get; set; }
    
    [XmlElement("puertoEmbarque")]
    public string? PuertoEmbarque { get; set; }
    
    [XmlElement("puertoDestino")]
    public string? PuertoDestino { get; set; }
    
    [XmlElement("paisDestino")]
    public string? PaisDestino { get; set; }
    
    [XmlElement("paisAdquisicion")]
    public string? PaisAdquisicion { get; set; }
    
    // 13. tipoIdentificacionComprador (REQUERIDO)
    [XmlElement("tipoIdentificacionComprador")]
    public string TipoIdentificacionComprador { get; set; } = string.Empty;
    
    // 14. guiaRemision (opcional)
    [XmlElement("guiaRemision")]
    public string? GuiaRemision { get; set; }
    
    // 15. razonSocialComprador (REQUERIDO)
    [XmlElement("razonSocialComprador")]
    public string RazonSocialComprador { get; set; } = string.Empty;
    
    // 16. identificacionComprador (REQUERIDO)
    [XmlElement("identificacionComprador")]
    public string IdentificacionComprador { get; set; } = string.Empty;
    
    // 17. direccionComprador (opcional)
    [XmlElement("direccionComprador")]
    public string? DireccionComprador { get; set; }
    
    // 18. totalSinImpuestos (REQUERIDO) ✅ AQUÍ ESTÁ EL ORDEN CORRECTO
    [XmlElement("totalSinImpuestos")]
    public string TotalSinImpuestos { get; set; } = string.Empty;
    
    // 19. totalSubsidio (opcional)
    [XmlElement("totalSubsidio")]
    public string? TotalSubsidio { get; set; }
    
    // 20. incoTermTotalSinImpuestos (opcional)
    [XmlElement("incoTermTotalSinImpuestos")]
    public string? IncoTermTotalSinImpuestos { get; set; }
    
    // 21. totalDescuento (REQUERIDO)
    [XmlElement("totalDescuento")]
    public string TotalDescuento { get; set; } = "0.00";
    
    // 22-25. Campos de reembolso (opcionales)
    [XmlElement("codDocReembolso")]
    public string? CodDocReembolso { get; set; }
    
    [XmlElement("totalComprobantesReembolso")]
    public string? TotalComprobantesReembolso { get; set; }
    
    [XmlElement("totalBaseImponibleReembolso")]
    public string? TotalBaseImponibleReembolso { get; set; }
    
    [XmlElement("totalImpuestoReembolso")]
    public string? TotalImpuestoReembolso { get; set; }
    
    // 26. totalConImpuestos (REQUERIDO)
    [XmlArray("totalConImpuestos")]
    [XmlArrayItem("totalImpuesto")]
    public List<TotalImpuesto> TotalConImpuestos { get; set; } = new();
    
    // 27. compensaciones (opcional)
    // [XmlElement("compensaciones")] - Omitir por ahora
    
    // 28. propina (opcional)
    [XmlElement("propina")]
    public string? Propina { get; set; }
    
    // 29-33. Campos de comercio exterior (opcionales)
    [XmlElement("fleteInternacional")]
    public string? FleteInternacional { get; set; }
    
    [XmlElement("seguroInternacional")]
    public string? SeguroInternacional { get; set; }
    
    [XmlElement("gastosAduaneros")]
    public string? GastosAduaneros { get; set; }
    
    [XmlElement("gastosTransporteOtros")]
    public string? GastosTransporteOtros { get; set; }
    
    // 34. importeTotal (REQUERIDO)
    [XmlElement("importeTotal")]
    public string ImporteTotal { get; set; } = string.Empty;
    
    // 35. moneda (opcional)
    [XmlElement("moneda")]
    public string? Moneda { get; set; }
    
    // 36. placa (opcional)
    [XmlElement("placa")]
    public string? Placa { get; set; }
    
    // 37. pagos (opcional)
    [XmlArray("pagos")]
    [XmlArrayItem("pago")]
    public List<FormaPago>? Pagos { get; set; }
    
    // 38-39. Retenciones (opcionales)
    [XmlElement("valorRetIva")]
    public string? ValorRetIva { get; set; }
    
    [XmlElement("valorRetRenta")]
    public string? ValorRetRenta { get; set; }
}
