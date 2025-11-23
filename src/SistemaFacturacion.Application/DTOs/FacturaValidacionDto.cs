namespace SistemaFacturacion.Application.DTOs;

public class FacturaValidacionDto
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool TieneXmlGenerado { get; set; }
    public string? EstadoValidacion { get; set; }
    public string? ClaveAcceso { get; set; }
}
