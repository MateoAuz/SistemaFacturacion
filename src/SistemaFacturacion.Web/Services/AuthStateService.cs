namespace SistemaFacturacion.Web.Services;

public class AuthStateService
{
    public string? Rol { get; set; }
    public string? NombreUsuario { get; set; }
    public int IdUsuario { get; set; }
    
    public bool EsAdmin => Rol == "Admin";
    
    public event Action? OnChange;
    
    public void SetUsuario(string rol, string nombreUsuario, int idUsuario)
    {
        Rol = rol;
        NombreUsuario = nombreUsuario;
        IdUsuario = idUsuario;
        OnChange?.Invoke();
    }
    
    public void Limpiar()
    {
        Rol = null;
        NombreUsuario = null;
        IdUsuario = 0;
        OnChange?.Invoke();
    }
}
