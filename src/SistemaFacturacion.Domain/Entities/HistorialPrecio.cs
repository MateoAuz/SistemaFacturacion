namespace SistemaFacturacion.Domain.Entities;

public class HistorialPrecio
{
    public int IdHistorial { get; set; }
    public int IdProducto { get; set; }
    public decimal? PrecioAnterior { get; set; }
    public decimal? PrecioNuevo { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.Now;
    public int? IdUsuario { get; set; }
    public string? Motivo { get; set; }

    public Producto? Producto { get; set; }
    public Usuario? Usuario { get; set; }
}
