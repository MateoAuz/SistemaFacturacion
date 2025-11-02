namespace SistemaFacturacion.Domain.Entities;

public class DetalleFactura
{
    public int IdDetalle { get; set; }
    public int IdFactura { get; set; }
    public int IdProducto { get; set; }
    public short Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal TotalLinea { get; set; }

    public Factura? Factura { get; set; }
    public Producto? Producto { get; set; }
}
