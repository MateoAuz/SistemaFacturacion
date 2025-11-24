using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SistemaFacturacion.Application.Contracts; // Asegúrate de usar el namespace correcto de la interfaz

namespace SistemaFacturacion.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // ✅ NUEVO MÉTODO: Envía PDF + XML
    public async Task EnviarFacturaConXmlAsync(
        string emailDestino,
        string nombreCliente,
        byte[] pdfBytes,
        byte[] xmlBytes,
        string numeroFactura,
        CancellationToken ct = default)
    {
        var message = new MimeMessage();

        // Configurar remitente
        var fromEmail = _configuration["Email:From"] ?? "no-reply@sistema.com";
        message.From.Add(new MailboxAddress("Sistema de Facturación", fromEmail));

        // Configurar destinatario
        message.To.Add(new MailboxAddress(nombreCliente, emailDestino));
        message.Subject = $"Factura Electrónica No. {numeroFactura}";

        var builder = new BodyBuilder
        {
            HtmlBody = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Estimado/a {nombreCliente},</h2>
                            <p>Adjuntamos su <strong>Factura Electrónica No. {numeroFactura}</strong> autorizada por el SRI.</p>
                            <p>Se incluyen dos archivos:</p>
                            <ul>
                                <li><strong>PDF (RIDE):</strong> Representación visual de la factura</li>
                                <li><strong>XML:</strong> Comprobante electrónico oficial con validez tributaria</li>
                            </ul>
                            <p>Estos documentos tienen validez legal ante el SRI.</p>
                            <br>
                            <p>Atentamente,</p>
                            <p><strong>Tu Empresa</strong></p>
                            <hr>
                            <p style='font-size: 12px; color: gray;'>
                                Este es un mensaje automático, por favor no responder.
                            </p>
                        </body>
                    </html>"
        };

        // ✅ Adjuntar PDF (RIDE)
        builder.Attachments.Add($"Factura-{numeroFactura}.pdf", pdfBytes, ContentType.Parse("application/pdf"));

        // ✅ Adjuntar XML Autorizado
        builder.Attachments.Add($"Factura-{numeroFactura}.xml", xmlBytes, ContentType.Parse("application/xml"));

        message.Body = builder.ToMessageBody();

        // Enviar correo
        using var client = new SmtpClient();

        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:Username"];
        var smtpPass = _configuration["Email:Password"];

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            throw new InvalidOperationException("La configuración de correo SMTP no está completa en appsettings.json");
        }

        try
        {
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(smtpUser, smtpPass, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error enviando correo a {emailDestino}: {ex.Message}", ex);
        }
    }

    public async Task EnviarFacturaAsync(
        string emailDestino,
        string nombreCliente,
        byte[] pdfBytes,
        string numeroFactura,
        CancellationToken ct = default)
    {
        var message = new MimeMessage();

        // Configurar remitente
        var fromEmail = _configuration["Email:From"] ?? "no-reply@sistema.com";
        message.From.Add(new MailboxAddress("Sistema de Facturación", fromEmail));

        // Configurar destinatario
        message.To.Add(new MailboxAddress(nombreCliente, emailDestino));

        message.Subject = $"Factura Electrónica No. {numeroFactura}";

        var builder = new BodyBuilder
        {
            HtmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Estimado/a {nombreCliente},</h2>
                    <p>Adjuntamos su Factura Electrónica No. <strong>{numeroFactura}</strong> autorizada por el SRI.</p>
                    <p>Este documento tiene validez tributaria.</p>
                    <br>
                    <p>Atentamente,</p>
                    <p><strong>Tu Empresa</strong></p>
                    <hr>
                    <p style='font-size: 12px; color: gray;'>
                        Este es un mensaje automático, por favor no responder.
                    </p>
                </body>
                </html>"
        };

        // Adjuntar el PDF
        builder.Attachments.Add($"Factura_{numeroFactura}.pdf", pdfBytes, ContentType.Parse("application/pdf"));

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        // Configuración del servidor SMTP
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:Username"];
        var smtpPass = _configuration["Email:Password"];

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            throw new InvalidOperationException("La configuración de correo (SMTP) no está completa en appsettings.json");
        }

        try
        {
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(smtpUser, smtpPass, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            // Lanzar excepción con más detalles para facilitar debugging
            throw new Exception($"Error enviando correo a {emailDestino}: {ex.Message}", ex);
        }
    }
}
