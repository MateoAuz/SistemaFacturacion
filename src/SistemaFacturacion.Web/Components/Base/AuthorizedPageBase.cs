using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SistemaFacturacion.Web.Components.Base;

public class AuthorizedPageBase : ComponentBase
{
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    
    protected string? RolUsuario { get; private set; }
    protected bool EsAdmin => RolUsuario == "Admin";
    protected bool EsVendedor => RolUsuario == "Vendedor";
    protected bool Autorizado { get; private set; }

    // Roles requeridos para esta página
    protected virtual string[]? RolesPermitidos { get; } = null;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await VerificarAutorizacion();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task VerificarAutorizacion()
    {
        try
        {
            var token = await JS.InvokeAsync<string>("localStorage.getItem", "authToken");
            RolUsuario = await JS.InvokeAsync<string>("localStorage.getItem", "userRole");

            // Si no hay token, redirigir al login
            if (string.IsNullOrEmpty(token))
            {
                Nav.NavigateTo("/login", true);
                return;
            }

            // Si hay roles específicos requeridos, verificar
            if (RolesPermitidos != null && RolesPermitidos.Length > 0)
            {
                if (string.IsNullOrEmpty(RolUsuario) || !RolesPermitidos.Contains(RolUsuario))
                {
                    Nav.NavigateTo("/acceso-denegado", true);
                    return;
                }
            }

            Autorizado = true;
            StateHasChanged();
        }
        catch
        {
            Nav.NavigateTo("/login", true);
        }
    }
}
