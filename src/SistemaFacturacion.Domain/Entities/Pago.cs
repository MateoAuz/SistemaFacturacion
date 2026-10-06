using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SistemaFacturacion.Domain.Entities;

public class Pago
{
    [Key]
    public int IdPago { get; set; }
    
    [Required]
    public int IdFactura { get; set; }
    
    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Column(TypeName = "decimal(12,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser positivo.")]
    public decimal Monto { get; set; }
    
    [Required(ErrorMessage = "El método de pago es obligatorio.")]
    [StringLength(50)]
    public string MetodoPago { get; set; } = string.Empty; 
    
    public DateTime FechaPago { get; set; } = DateTime.Now;
    
    [Required(ErrorMessage = "El usuario que registra el pago es obligatorio.")]
    public int IdUsuario { get; set; }
    
    [Required]
    [StringLength(15)]
    public string Estado { get; set; } = "REGISTRADO"; 
    
    // Navegación
    [JsonIgnore]
    public Factura? Factura { get; set; }
    public Usuario? Usuario { get; set; }
}