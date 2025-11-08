using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IStockService
{
    /// <summary>
    /// Valida si hay suficiente stock para una lista de detalles de factura.
    /// </summary>
    /// <returns>Una tupla (bool EsValido, string MensajeDeError)</returns>
    Task<(bool, string)> ValidarStockAsync(IEnumerable<DetalleFactura> detalles, CancellationToken ct = default);

    /// <summary>
    /// Descuenta el stock de los productos vendidos.
    /// </summary>
    Task DescontarStockAsync(IEnumerable<DetalleFactura> detalles, CancellationToken ct = default);
}