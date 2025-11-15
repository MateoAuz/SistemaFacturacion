namespace SistemaFacturacion.Application.Contracts;

public interface IClaveAccesoService
{
    string GenerarClaveAcceso(
        DateTime fechaEmision,
        string tipoComprobante,
        string ruc,
        string ambiente,
        string establecimiento,
        string puntoEmision,
        string secuencial);
}
