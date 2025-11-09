using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SistemaFacturacion.Domain.Entities;

public class Cliente
{
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "El tipo de identificación es obligatorio")]
    [RegularExpression("^(CEDUL|RUC)$", ErrorMessage = "El tipo de identificación debe ser CEDUL o RUC")]
    public string TipoIdentificacion { get; set; } = "CEDUL";

    [Required(ErrorMessage = "La identificación es obligatoria")]
    [StringLength(13, MinimumLength = 10, ErrorMessage = "La identificación debe tener entre 10 y 13 dígitos")]
    [RegularExpression(@"^\d+$", ErrorMessage = "La identificación solo puede contener números")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(60, ErrorMessage = "Los nombres no pueden exceder 60 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Los nombres solo pueden contener letras y espacios")]
    public string Nombres { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "Los apellidos no pueden exceder 60 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]*$", ErrorMessage = "Los apellidos solo pueden contener letras y espacios")]
    public string? Apellidos { get; set; }

    [StringLength(120, ErrorMessage = "La dirección no puede exceder 120 caracteres")]
    public string? Direccion { get; set; }

    [StringLength(15, ErrorMessage = "El teléfono no puede exceder 15 caracteres")]
    [RegularExpression(@"^[\d\s\+\-\(\)]*$", ErrorMessage = "El teléfono solo puede contener números, espacios y los caracteres + - ( )")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    [StringLength(80, ErrorMessage = "El correo no puede exceder 80 caracteres")]
    public string? Correo { get; set; }

    public bool Estado { get; set; } = true;

    [JsonIgnore]
    public ICollection<Factura>? Facturas { get; set; }

    // Método para validación personalizada de cédula/RUC
    public ValidationResult ValidarIdentificacion()
    {
        if (TipoIdentificacion == "CEDUL" && Identificacion.Length != 10)
            return new ValidationResult("La cédula debe tener exactamente 10 dígitos");
        
        if (TipoIdentificacion == "RUC" && Identificacion.Length != 13)
            return new ValidationResult("El RUC debe tener exactamente 13 dígitos");
        
        // Validar cédula ecuatoriana
        if (TipoIdentificacion == "CEDUL" && !ValidarCedulaEcuatoriana(Identificacion))
            return new ValidationResult("El número de cédula no es válido");
            
        return ValidationResult.Success;
    }

    private bool ValidarCedulaEcuatoriana(string cedula)
    {
        if (cedula.Length != 10 || !cedula.All(char.IsDigit))
            return false;

        // Verificar que los primeros dos dígitos sean válidos (provincia)
        int provincia = int.Parse(cedula.Substring(0, 2));
        if (provincia < 1 || provincia > 24)
            return false;

        // Algoritmo de validación de cédula ecuatoriana
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
}