using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IFacturaRepository
{
    Task<Factura> CrearFacturaAsync(Factura factura, CancellationToken ct = default);
    
    // MÉTODOS AÑADIDOS
    Task<Factura?> GetByIdAsync(int id, bool includePagos = false, bool includeDetalles = false, bool includeCliente = false, CancellationToken ct = default);
    Task UpdateEstadoAndSaldoAsync(int idFactura, string nuevoEstado, decimal nuevoSaldoPendiente, CancellationToken ct = default);
    Task<IReadOnlyList<Factura>> GetAllAsync(string? estado = null, bool includeCliente = true, bool includeDetalles = false, CancellationToken ct = default); 
    Task<List<Factura>> GetFacturasPagadasAsync(bool includeCliente = true, CancellationToken ct = default);

}