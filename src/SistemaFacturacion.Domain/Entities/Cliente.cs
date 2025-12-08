using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SistemaFacturacion.Domain.Entities;

public class Cliente
{
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "El tipo de identificación es obligatorio")]
    [RegularExpression("^(CEDUL|RUC|PASAP)$", ErrorMessage = "El tipo debe ser Cédula, RUC o Pasaporte")]
    public string TipoIdentificacion { get; set; } = "CEDUL";

    [Required(ErrorMessage = "La identificación es obligatoria")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "La identificación debe tener entre 5 y 12 caracteres")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(60, ErrorMessage = "Los nombres no pueden exceder 60 caracteres")]
    public string Nombres { get; set; } = string.Empty;

    [StringLength(60)]
    public string? Apellidos { get; set; }

    [StringLength(120)]
    public string? Direccion { get; set; }

    [StringLength(15)]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
    [StringLength(80)]
    public string? Correo { get; set; }

    public bool Estado { get; set; } = true;

    [JsonIgnore]
    public ICollection<Factura>? Facturas { get; set; }

    public string? ValidarIdentificacion()
    {
        var id = Identificacion?.Trim();
        if (string.IsNullOrEmpty(id)) return "La identificación es obligatoria.";

        if (TipoIdentificacion == "CEDUL")
        {
            if (id.Length != 10 || !long.TryParse(id, out _))
                return "La cédula debe tener exactamente 10 dígitos numéricos.";
            
            if (!ValidarCedulaEcuatoriana(id))
                return "El número de cédula es inválido.";
        }
        else if (TipoIdentificacion == "RUC")
        {
            if (id.Length != 13 || !long.TryParse(id, out _))
                return "El RUC debe tener exactamente 13 dígitos numéricos.";
            
            if (!id.EndsWith("001"))
                return "El RUC debe terminar en 001.";
        }
        else if (TipoIdentificacion == "PASAP")
        {
            if (id.Length < 5 || id.Length > 20)
                return "El pasaporte debe tener entre 5 y 20 caracteres.";
            
            if (!Regex.IsMatch(id, @"^[a-zA-Z0-9]+$"))
                 return "El pasaporte solo puede contener letras y números.";
        }

        return null; 
    }

    private bool ValidarCedulaEcuatoriana(string cedula)
    {
        try 
        {
            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || provincia > 24) return false;

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito >= 6) return false; 

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int total = 0;

            for (int i = 0; i < 9; i++)
            {
                int valor = int.Parse(cedula[i].ToString()) * coeficientes[i];
                total += valor > 9 ? valor - 9 : valor;
            }

            int digitoVerificador = (10 - (total % 10)) % 10;
            return digitoVerificador == int.Parse(cedula[9].ToString());
        }
        catch 
        {
            return false;
        }
    }
}