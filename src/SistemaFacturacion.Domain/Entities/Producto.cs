using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaFacturacion.Domain.Entities
{
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(40, ErrorMessage = "Máximo 40 caracteres.")]
        public string? Categoria { get; set; }

        [Required(ErrorMessage = "Debe ingresar el precio unitario.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Debe ser un valor mayor a 0.")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "Debe ingresar el precio de venta.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Debe ser un valor mayor a 0.")]
        public decimal PrecioVenta { get; set; }

        [Required(ErrorMessage = "Debe ingresar el stock.")]
        [Range(0, 9999, ErrorMessage = "El stock debe estar entre 0 y 9999.")]
        public short StockActual { get; set; }

        public DateTime? FechaExpiracion { get; set; }

        public bool Estado { get; set; } = true;
    }
}
