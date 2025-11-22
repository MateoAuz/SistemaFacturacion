namespace SistemaFacturacion.Domain.Configuration;

public class SriConfiguracion
{
    public string RutaXsdFactura { get; set; } = "Resources/XSD/factura.xsd";
    public string Ambiente { get; set; } = "1";
    public string UrlRecepcion { get; set; } = string.Empty;
    public string UrlAutorizacion { get; set; } = string.Empty;
}
