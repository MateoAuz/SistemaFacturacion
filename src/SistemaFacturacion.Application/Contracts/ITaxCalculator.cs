namespace SistemaFacturacion.Application.Contracts;

public interface ITaxCalculator
{
    CalculoImpuestoResult CalcularImpuestos(decimal subtotal);
}

public record CalculoImpuestoResult(
    decimal Subtotal,
    decimal MontoIva,
    decimal Total
);