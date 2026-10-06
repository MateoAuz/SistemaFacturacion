namespace SistemaFacturacion.Domain.Entities;

public class ConfiguracionEmpresa
{
    public int IdConfiguracion { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string? DireccionMatriz { get; set; }
    public string? Establecimiento { get; set; } 
    public string? PuntoEmision { get; set; }
    public char Ambiente { get; set; } 
    public string? RutaCertificado { get; set; }
    public string? ClaveCertificado { get; set; }
    public string? CorreoEmpresa { get; set; }
    public string? ObligadoContabilidad { get; set; } 
}
