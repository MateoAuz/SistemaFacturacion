namespace SistemaFacturacion.Domain.Entities;

public class Factura
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaEmision { get; set; } = DateTime.Now;
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "PENDIENTE";

    public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();


    public Cliente? Cliente { get; set; }
    public Usuario? Usuario { get; set; }
    public ICollection<DetalleFactura>? DetallesFactura { get; set; }
    public ComprobanteElectronico? Comprobante { get; set; }
}
