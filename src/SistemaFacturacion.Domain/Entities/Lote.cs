using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SistemaFacturacion.Domain.Entities
{
    public class Lote
    {
        [Key]
        public int IdLote { get; set; }

        [Required(ErrorMessage = "El número de lote es obligatorio.")]
        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        public string NumeroLote { get; set; } = string.Empty;

        [Required]
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
        [Column(TypeName = "decimal(12,2)")]
        public decimal PrecioCompra { get; set; }

        // Navigation properties
        [ForeignKey("ProductoId")]
        [JsonIgnore]
        public Producto Producto { get; set; } = null!;
        
        // ✅ AGREGAR: Propiedades calculadas
        [NotMapped]
        public bool EstaVencido => FechaExpiracion.HasValue && FechaExpiracion.Value < DateTime.Now.Date;
        
        [NotMapped]
        public bool EstaProximoAVencer => FechaExpiracion.HasValue && 
                                         FechaExpiracion.Value <= DateTime.Now.AddDays(30).Date && 
                                         FechaExpiracion.Value > DateTime.Now.Date;
        
        [NotMapped]
        public int DiasParaVencer => FechaExpiracion.HasValue ? 
            (int)(FechaExpiracion.Value - DateTime.Now.Date).TotalDays : int.MaxValue;
    }
}