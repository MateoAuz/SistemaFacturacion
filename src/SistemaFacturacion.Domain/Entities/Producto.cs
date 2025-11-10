using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // ✅ AGREGAR esta línea
using System.Linq; // ✅ AGREGAR para usar Where y Sum

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

        [Required(ErrorMessage = "Debe ingresar el precio de venta.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Debe ser un valor mayor a 0.")]
        public decimal PrecioVenta { get; set; }

        public bool Estado { get; set; } = true;

        // ✅ AGREGAR: Relación con lotes
        public ICollection<Lote> Lotes { get; set; } = new List<Lote>();

        // ✅ AGREGAR: Propiedad calculada para stock total
        [NotMapped] // ✅ Ahora funciona con el using correcto
        public int StockTotal => Lotes.Where(l => l.CantidadActual > 0).Sum(l => l.CantidadActual);
        
        // ✅ AGREGAR: Propiedad para saber si tiene stock
        [NotMapped]
        public bool TieneStock => StockTotal > 0;
        
        // ✅ AGREGAR: Propiedad para lotes próximos a vencer
        [NotMapped]
        public IEnumerable<Lote> LotesProximosAVencer => 
            Lotes.Where(l => l.FechaExpiracion.HasValue && 
                            l.FechaExpiracion.Value <= DateTime.Now.AddDays(30));
    }
}