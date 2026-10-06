using Microsoft.AspNetCore.Mvc;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _repo;

    public ClientesController(IClienteRepository repo)
    {
        _repo = repo;
    }

    // GET /api/clientes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetAll(CancellationToken ct)
    {
        var list = await _repo.GetAllActivosAsync(ct);
        return Ok(list);
    }

    // GET /api/clientes/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetById(int id, CancellationToken ct)
    {
        try
        {
            var c = await _repo.GetByIdAsync(id, ct);
            if (c is null)
            {
                Console.WriteLine($"Cliente con ID {id} no encontrado");
                return NotFound();
            }
            Console.WriteLine($"Cliente encontrado: {c.Nombres} {c.Apellidos}");
            return Ok(c);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetById: {ex}");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET /api/clientes/by-identificacion/{id}
    [HttpGet("by-identificacion/{identificacion}")]
    public async Task<ActionResult<Cliente>> GetByIdentificacion(string identificacion, CancellationToken ct)
    {
        var c = await _repo.GetByIdentificacionAsync(identificacion, ct);
        if (c is null) return NotFound();
        return Ok(c);
    }

    // POST /api/clientes
    [HttpPost]
    public async Task<ActionResult<Cliente>> Create([FromBody] Cliente cliente, CancellationToken ct)
    {
        var created = await _repo.AddAsync(cliente, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdCliente }, created);
    }

    // PUT /api/clientes/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Cliente>> Update(int id, [FromBody] Cliente cliente, CancellationToken ct)
    {
        var updated = await _repo.UpdateAsync(id, cliente, ct);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    // DELETE /api/clientes/{id} 
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await _repo.DeleteLogicalAsync(id, ct);
        if (!ok) return NotFound();
        return NoContent();
    }

    // GET /api/clientes/inactivos
    [HttpGet("inactivos")]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetInactivos(CancellationToken ct)
    {
        var list = await _repo.GetAllInactivosAsync(ct);
        return Ok(list);
    }

    // PATCH /api/clientes/{id}/reactivar
    [HttpPatch("{id:int}/reactivar")]
    public async Task<IActionResult> Reactivar(int id, CancellationToken ct)
    {
        var result = await _repo.ReactivarAsync(id, ct);
        if (!result) return NotFound();
        return NoContent();
    }

    // GET /api/clientes/todos 
    [HttpGet("todos")]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetAllIncluyendoInactivos(CancellationToken ct)
    {
        var list = await _repo.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpGet("existe/{identificacion}")]
    public async Task<ActionResult<bool>> ExisteIdentificacion(string identificacion)
    {
        var cliente = await _repo.GetByIdentificacionAsync(identificacion);
        return Ok(cliente != null);
    }
}
