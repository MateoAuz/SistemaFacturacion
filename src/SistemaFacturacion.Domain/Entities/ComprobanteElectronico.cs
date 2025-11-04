namespace SistemaFacturacion.Domain.Entities;

public class ComprobanteElectronico
{
    public int IdComprobante { get; set; }
    public int IdFactura { get; set; }
    public string? ClaveAcceso { get; set; }
    public string? XmlGenerado { get; set; }
    public string? XmlFirmado { get; set; }
    public string EstadoEnvio { get; set; } = "NO_ENVIADO";
    public string? MensajeRespuesta { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaAutorizacion { get; set; }
    public string? NumeroAutorizacion { get; set; }

    public Factura Factura { get; set; } = null!;

}
