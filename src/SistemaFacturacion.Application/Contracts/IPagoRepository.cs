using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IPagoRepository
{
    Task<Pago> AddAsync(Pago pago, CancellationToken ct = default);
    Task<IReadOnlyList<Pago>> GetByFacturaIdAsync(int facturaId, CancellationToken ct = default);
    Task<Pago?> GetByIdAsync(int idPago, CancellationToken ct = default);
    Task<bool> UpdateEstadoAsync(int idPago, string nuevoEstado, CancellationToken ct = default);
}

public interface IPagoService
{
    Task<Pago> RegistrarPagoAsync(int idFactura, decimal monto, string metodoPago, int idUsuario, CancellationToken ct = default);
}