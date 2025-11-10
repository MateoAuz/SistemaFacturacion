using System.ComponentModel.DataAnnotations;

namespace SistemaFacturacion.Domain.Entities
{
    public class LoteCreateDto
    {
        [Required(ErrorMessage = "El número de lote es obligatorio.")]
        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        public string NumeroLote { get; set; } = string.Empty;

        [Required(ErrorMessage = "El producto es obligatorio.")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public DateTime? FechaExpiracion { get; set; }

        [Required(ErrorMessage = "La cantidad inicial es obligatoria.")]
        [Range(1, 9999, ErrorMessage = "La cantidad debe estar entre 1 y 9999.")]
        public int CantidadInicial { get; set; }

        [Required]
        [Range(0, 9999)]
        public int CantidadActual { get; set; }

        [Required(ErrorMessage = "El precio de compra es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Debe ser un valor mayor a 0.")]
        public decimal PrecioCompra { get; set; }
    }
}