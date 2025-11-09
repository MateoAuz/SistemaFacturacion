using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionRepository _repo;

    public ConfiguracionController(IConfiguracionRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<ConfiguracionEmpresa>> Get(CancellationToken ct)
    {
        var config = await _repo.GetConfiguracionAsync(ct);
        if (config == null)
        {
            return Ok(new ConfiguracionEmpresa());
        }
        return Ok(config);
    }

    [HttpPost]
    public async Task<ActionResult<ConfiguracionEmpresa>> Save([FromBody] ConfiguracionEmpresa config, CancellationToken ct)
    {
        if (config == null || string.IsNullOrWhiteSpace(config.Ruc))
        {
            return BadRequest("Se requieren datos de configuración.");
        }
        
        var configGuardada = await _repo.SaveConfiguracionAsync(config, ct);
        return Ok(configGuardada);
    }
}