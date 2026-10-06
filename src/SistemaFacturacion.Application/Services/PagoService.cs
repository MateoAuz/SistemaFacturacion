using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using System.Transactions;

namespace SistemaFacturacion.Application.Services;

public class PagoService : IPagoService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IPagoRepository _pagoRepo;

    public PagoService(IFacturaRepository facturaRepo, IPagoRepository pagoRepo)
    {
        _facturaRepo = facturaRepo;
        _pagoRepo = pagoRepo;
    }

    public async Task<Pago> RegistrarPagoAsync(int idFactura, decimal monto, string metodoPago, int idUsuario, CancellationToken ct = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var factura = await _facturaRepo.GetByIdAsync(idFactura, includePagos: false, ct: ct);
        if (factura is null) throw new InvalidOperationException("Factura no encontrada.");
        
        if (factura.Estado == "ANULADA") 
            throw new InvalidOperationException("No se puede registrar pagos a facturas anuladas.");

        if (monto > factura.SaldoPendiente)
            throw new InvalidOperationException($"El pago de {monto:C} excede el saldo pendiente de {factura.SaldoPendiente:C}.");
        
        if (monto <= 0)
            throw new InvalidOperationException("El monto del pago debe ser mayor a cero.");


        var nuevoPago = new Pago
        {
            IdFactura = idFactura,
            Monto = monto,
            MetodoPago = metodoPago,
            IdUsuario = idUsuario,
            FechaPago = DateTime.UtcNow.AddHours(-5)
        };

        var pagoRegistrado = await _pagoRepo.AddAsync(nuevoPago, ct);
        
        var nuevoSaldo = factura.SaldoPendiente - monto;
        string nuevoEstado;

        if (nuevoSaldo <= 0)
        {
            nuevoEstado = "PAGADA"; 
            nuevoSaldo = 0; 
        }
        else
        {
            nuevoEstado = "PENDIENTE"; 
        }

        await _facturaRepo.UpdateEstadoAndSaldoAsync(idFactura, nuevoEstado, nuevoSaldo, ct);

        scope.Complete();
        return pagoRegistrado;
    }
    
}