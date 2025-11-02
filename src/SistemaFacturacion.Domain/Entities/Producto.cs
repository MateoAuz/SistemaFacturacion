namespace SistemaFacturacion.Domain.Entities;

public class Producto
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioVenta { get; set; }
    public short StockActual { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool Estado { get; set; } = true;

    public ICollection<DetalleFactura>? DetallesFactura { get; set; }
    public ICollection<HistorialPrecio>? HistorialPrecios { get; set; }
}
