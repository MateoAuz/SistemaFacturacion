using System.Text.Json.Serialization;
namespace SistemaFacturacion.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ClaveHash { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public char Rol { get; set; }  
    public bool Estado { get; set; } = true;
    
    [JsonIgnore] 

    public ICollection<Factura>? Facturas { get; set; }
    public ICollection<HistorialPrecio>? HistorialPrecios { get; set; }
}
