using System.Threading;
using System.Threading.Tasks;

namespace SistemaFacturacion.Application.Contracts; // O .Services si prefieres

public interface IEmailService
{
    Task EnviarFacturaAsync(
        string emailDestino, 
        string nombreCliente, 
        byte[] pdfBytes, 
        string numeroFactura, 
        CancellationToken ct = default);
}
