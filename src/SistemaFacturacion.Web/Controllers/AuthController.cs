// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Simulación simple; en producción valida contra BD con hash
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        if (dto.Usuario == "admin" && dto.Password == "admin123")
        {
            return Ok(new { success = true, usuario = dto.Usuario });
        }

        return Unauthorized(new { success = false, message = "Credenciales incorrectas" });
    }
}

public record LoginDto(string Usuario, string Password);
