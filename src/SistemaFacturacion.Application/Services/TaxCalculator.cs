using SistemaFacturacion.Application.Contracts;

namespace SistemaFacturacion.Application.Services;

public class TaxCalculator : ITaxCalculator
{
    // Tasa de IVA 15% (Ecuador)
    private const decimal TASA_IVA = 0.15m;

    public CalculoImpuestoResult CalcularImpuestos(decimal subtotal)
    {
        var montoIva = Math.Round(subtotal * TASA_IVA, 2);
        var total = subtotal + montoIva;

        return new CalculoImpuestoResult(
            Subtotal: subtotal,
            MontoIva: montoIva,
            Total: total
        );
    }
}