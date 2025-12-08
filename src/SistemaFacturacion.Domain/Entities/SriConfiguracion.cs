namespace SistemaFacturacion.Domain.Configuration;

public class SriConfiguracion
{
    public string RutaXsdFactura { get; set; } = "Resources/XSD/factura_V2.1.0.xsd";
    public string Ambiente { get; set; } = "1"; 
    
    public string UrlRecepcionPruebas { get; set; } = 
        "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
    
    public string UrlAutorizacionPruebas { get; set; } = 
        "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";
    
}
