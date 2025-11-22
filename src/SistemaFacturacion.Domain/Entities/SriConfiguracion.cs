namespace SistemaFacturacion.Domain.Configuration;

public class SriConfiguracion
{
    public string RutaXsdFactura { get; set; } = "Resources/XSD/factura.xsd";
    public string Ambiente { get; set; } = "1"; // 1=Pruebas, 2=Producción
    
    // URLs para ambiente de pruebas (CELCER)
    public string UrlRecepcionPruebas { get; set; } = 
        "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
    
    public string UrlAutorizacionPruebas { get; set; } = 
        "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";
    
}
