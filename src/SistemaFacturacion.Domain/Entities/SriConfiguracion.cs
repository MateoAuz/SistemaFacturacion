namespace SistemaFacturacion.Domain.Configuration;

public class SriConfiguracion
{
    public string RutaXsdFactura { get; set; } = "Resources/XSD/factura_V2.1.0.xsd";
    public string Ambiente { get; set; } = "1";
    public string UrlRecepcion { get; set; } = string.Empty;
    public string UrlAutorizacion { get; set; } = string.Empty;
}
