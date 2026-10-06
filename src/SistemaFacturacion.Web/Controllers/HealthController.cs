using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public HealthController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Get()
    {
        var hasClientes = _db.Clientes.Any();
        var hasConfig   = _db.ConfiguracionEmpresa.Any();
        return Ok(new { ok = true, clientes = hasClientes, configuracion = hasConfig });
    }
}
