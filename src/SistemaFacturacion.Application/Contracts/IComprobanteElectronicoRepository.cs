using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Contracts;

public interface IComprobanteElectronicoRepository
{
    Task<ComprobanteElectronico?> GetByFacturaIdAsync(int idFactura, CancellationToken ct = default);
    Task<ComprobanteElectronico?> GetByClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<ComprobanteElectronico> AddAsync(ComprobanteElectronico comprobante, CancellationToken ct = default);
    Task<ComprobanteElectronico> UpdateAsync(ComprobanteElectronico comprobante, CancellationToken ct = default);
    Task<List<ComprobanteElectronico>> GetByEstadoAsync(string estado, CancellationToken ct = default);
}
