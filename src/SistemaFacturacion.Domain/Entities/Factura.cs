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
    public decimal SaldoPendiente { get; set; } = 0;

    public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();


    public Cliente? Cliente { get; set; }
    public Usuario? Usuario { get; set; }
    public ComprobanteElectronico? Comprobante { get; set; }
}
