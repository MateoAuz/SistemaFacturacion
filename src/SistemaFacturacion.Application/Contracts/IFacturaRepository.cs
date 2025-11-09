using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IFacturaRepository
{
    /// <summary>
    /// Guarda una factura y sus detalles en una transacción, y descuenta el stock.
    /// </summary>
    Task<Factura> CrearFacturaAsync(Factura factura, CancellationToken ct = default);
    
    // (Aquí irían otros métodos como GetFacturaAsync, AnularFacturaAsync, etc.)
}