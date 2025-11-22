namespace SistemaFacturacion.Application.DTOs;

public class FacturaElectronicaDto
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
    public string ClaveAcceso { get; set; } = string.Empty;
    public string EstadoEnvio { get; set; } = string.Empty;
    public bool TieneXmlFirmado { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaAutorizacion { get; set; }
}
