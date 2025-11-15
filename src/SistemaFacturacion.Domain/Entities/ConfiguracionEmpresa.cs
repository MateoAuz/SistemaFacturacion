namespace SistemaFacturacion.Domain.Entities;

public class ConfiguracionEmpresa
{
    public int IdConfiguracion { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string? DireccionMatriz { get; set; }
    public string? Establecimiento { get; set; } // NUEVO: 001, 002, etc.
    public string? PuntoEmision { get; set; }
    public char Ambiente { get; set; }  // '1' o '2'
    public string? RutaCertificado { get; set; }
    public string? ClaveCertificado { get; set; }
    public string? CorreoEmpresa { get; set; }
    public string? ObligadoContabilidad { get; set; } // SI o NO
}
