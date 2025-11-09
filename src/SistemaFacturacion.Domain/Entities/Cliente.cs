using System.Text.Json.Serialization;
namespace SistemaFacturacion.Domain.Entities;

public class Cliente
{
    public int IdCliente { get; set; }
    public string TipoIdentificacion { get; set; } = "CEDUL";
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public bool Estado { get; set; } = true;

    [JsonIgnore] // <-- AÑADE ESTO
    public ICollection<Factura>? Facturas { get; set; }

}
