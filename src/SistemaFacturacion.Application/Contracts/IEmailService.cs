using System.Threading;
using System.Threading.Tasks;

namespace SistemaFacturacion.Application.Contracts; 

public interface IEmailService
{
    
    Task EnviarFacturaConXmlAsync(
        string emailDestino,
        string nombreCliente,
        byte[] pdfBytes,
        byte[] xmlBytes,
        string numeroFactura,
        CancellationToken ct = default);
    Task EnviarFacturaAsync(
        string emailDestino,
        string nombreCliente,
        byte[] pdfBytes,
        string numeroFactura,
        CancellationToken ct = default);
}
