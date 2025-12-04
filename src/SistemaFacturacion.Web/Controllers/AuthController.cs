using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthController(IConfiguration config, IUsuarioRepository usuarioRepository)
    {
        _config = config;
        _usuarioRepository = usuarioRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)  // ✅ Agregar <IActionResult>
    {
        var usuario = await _usuarioRepository.ValidarCredencialesAsync(dto.Usuario, dto.Password);
        
        if (usuario == null)
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

        var token = GenerarToken(usuario);
        
        var rolCompleto = usuario.Rol switch
        {
            'A' => "Admin",
            'V' => "Vendedor",
            _ => "Vendedor"
        };

        return Ok(new
        {
            token,
            nombreUsuario = usuario.NombreUsuario,
            rol = rolCompleto,
            idUsuario = usuario.IdUsuario
        });
    }

    private string GenerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var rolCompleto = usuario.Rol switch
        {
            'A' => "Admin",
            'V' => "Vendedor",
            _ => "Vendedor"
        };
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.NombreUsuario),
            new Claim("rol", rolCompleto),
            new Claim("id", usuario.IdUsuario.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record LoginDto(string Usuario, string Password);
}
