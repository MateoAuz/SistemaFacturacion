using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SistemaFacturacion.Web.Services;

namespace SistemaFacturacion.Web;

public class SimpleAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenHolder _tokenHolder;

    public SimpleAuthStateProvider(TokenHolder tokenHolder)
    {
        _tokenHolder = tokenHolder;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsIdentity identity;

        if (!string.IsNullOrEmpty(_tokenHolder.Token))
        {
            // Usuario autenticado (no hace falta parsear todo el JWT para tu caso)
            identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Usuario"),
            }, "custom");
        }
        else
        {
            // Usuario anónimo
            identity = new ClaimsIdentity();
        }

        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }

    public void MarkUserAsAuthenticated(string username, string role)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        }, "custom");

        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(anonymous)));
    }
}
