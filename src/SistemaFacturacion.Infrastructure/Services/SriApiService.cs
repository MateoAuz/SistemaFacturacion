using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Configuration;

namespace SistemaFacturacion.Infrastructure.Services;

public class SriApiService : ISriApiService
{
    private readonly HttpClient _httpClient;
    private readonly SriConfiguracion _config;

    private const string URL_RECEPCION_PRUEBAS = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
    private const string URL_AUTORIZACION_PRUEBAS = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";

    public SriApiService(IHttpClientFactory httpClientFactory, SriConfiguracion config)
    {
        _httpClient = httpClientFactory.CreateClient("SriSoap");
        _config = config;
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<(bool Success, string Message)> EnviarComprobanteAsync(
        string xmlFirmado,
        string claveAcceso,
        CancellationToken ct = default)
    {
        try
        {


            var xmlBytes = Encoding.UTF8.GetBytes(xmlFirmado); 
            var xmlBase64 = Convert.ToBase64String(xmlBytes);

            // Construir el SOAP Envelope con el base64 dentro de CDATA
            var soapEnvelope = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                  xmlns:ec=""http://ec.gob.sri.ws.recepcion"">
    <soapenv:Header/>
    <soapenv:Body>
        <ec:validarComprobante>
            <xml><![CDATA[{xmlBase64}]]></xml>
        </ec:validarComprobante>
    </soapenv:Body>
</soapenv:Envelope>";

            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

            var response = await _httpClient.PostAsync(URL_RECEPCION_PRUEBAS, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            Console.WriteLine("=== RESPUESTA DEL SRI (Recepción) ===");
            Console.WriteLine(responseBody);
            Console.WriteLine("====================================");

            if (!response.IsSuccessStatusCode)
            {
                return (false, $"❌ Error HTTP {response.StatusCode}: {response.ReasonPhrase}");
            }

            var doc = XDocument.Parse(responseBody);

            var namespaces = new[]
            {
                XNamespace.Get("http://ec.gob.sri.ws.recepcion"),
                XNamespace.Get("http://ec.gob.sri.ws.recepcion/recepcion"),
                XNamespace.None
            };

            XElement? respuestaElement = null;
            foreach (var ns in namespaces)
            {
                respuestaElement = doc.Descendants(ns + "RespuestaRecepcionComprobante").FirstOrDefault();
                if (respuestaElement != null) break;
            }

            if (respuestaElement == null)
            {
                var estadoElement = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "estado");
                if (estadoElement != null)
                {
                    var estado = estadoElement.Value;

                    if (estado == "RECIBIDA")
                    {
                        return (true, "✅ Comprobante recibido exitosamente por el SRI");
                    }
                    else if (estado == "DEVUELTA")
                    {
                        var mensajesElements = doc.Descendants().Where(e => e.Name.LocalName == "mensaje");
                        var errorList = mensajesElements.Select(m => m.Value).ToList();
                        var errorMsg = "❌ El SRI devolvió el comprobante con errores:\n" +
                                       string.Join("\n", errorList);
                        return (false, errorMsg);
                    }

                    return (false, $"⚠️ Estado del SRI: {estado}");
                }

                return (false, $"❌ No se pudo parsear la respuesta del SRI. Revisa los logs de la consola.");
            }

            var ns2 = respuestaElement.Name.Namespace;
            var estado2 = respuestaElement.Element(ns2 + "estado")?.Value;

            if (estado2 == "RECIBIDA")
            {
                return (true, "✅ Comprobante recibido exitosamente por el SRI");
            }
            else if (estado2 == "DEVUELTA")
            {
                var comprobantes = respuestaElement.Element(ns2 + "comprobantes");
                var comprobante = comprobantes?.Element(ns2 + "comprobante");
                var mensajes = comprobante?.Element(ns2 + "mensajes");

                var errorList = mensajes?.Elements(ns2 + "mensaje")
                    .Select(m =>
                    {
                        var mensaje = m.Element(ns2 + "mensaje")?.Value;
                        var tipo = m.Element(ns2 + "tipo")?.Value;
                        return $"[{tipo}] {mensaje}";
                    })
                    .ToList() ?? new List<string>();

                var errorMsg = "❌ El SRI devolvió el comprobante con errores:\n" +
                               string.Join("\n", errorList);

                return (false, errorMsg);
            }

            return (false, $"⚠️ Respuesta inesperada del SRI: {estado2 ?? "Sin estado"}");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"❌ Error de conexión con el SRI: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"❌ Error inesperado: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }

    public async Task<(bool Success, string Message, string? XmlAutorizado, string? NumeroAutorizacion)> 
    ConsultarAutorizacionAsync(string claveAcceso, CancellationToken ct = default)
{
    try
    {
        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                  xmlns:ec=""http://ec.gob.sri.ws.autorizacion"">
    <soapenv:Header/>
    <soapenv:Body>
        <ec:autorizacionComprobante>
            <claveAccesoComprobante>{claveAcceso}</claveAccesoComprobante>
        </ec:autorizacionComprobante>
    </soapenv:Body>
</soapenv:Envelope>";

        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        var response = await _httpClient.PostAsync(URL_AUTORIZACION_PRUEBAS, content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        Console.WriteLine("=== RESPUESTA DEL SRI (Autorización) ===");
        Console.WriteLine(responseBody);
        Console.WriteLine("========================================");

        if (!response.IsSuccessStatusCode)
        {
            return (false, $"❌ Error HTTP {response.StatusCode}: {response.ReasonPhrase}", null, null);
        }

        var doc = XDocument.Parse(responseBody);

        var autorizacionElement = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "autorizacion");

        if (autorizacionElement == null)
        {
            return (false, "⚠️ No se encontró información de autorización para esta clave de acceso", null, null);
        }

        var ns = autorizacionElement.Name.Namespace;
        var estado = autorizacionElement.Element(ns + "estado")?.Value;
        var numeroAutorizacion = autorizacionElement.Element(ns + "numeroAutorizacion")?.Value;
        var fechaAutorizacion = autorizacionElement.Element(ns + "fechaAutorizacion")?.Value;
        var xmlAutorizado = autorizacionElement.ToString();

        if (estado == "AUTORIZADO")
        {
            var msg = $"✅ Comprobante AUTORIZADO\n📋 Número: {numeroAutorizacion}\n📅 Fecha: {fechaAutorizacion}";
            
            return (true, msg, xmlAutorizado, numeroAutorizacion);
        }
        else if (estado == "NO AUTORIZADO" || estado == "DEVUELTA")
        {
            var mensajes = autorizacionElement.Element(ns + "mensajes");
            var errorList = mensajes?.Elements(ns + "mensaje")
                .Select(m =>
                {
                    var codigo = m.Element(ns + "identificador")?.Value ?? "";
                    var mensaje = m.Element(ns + "mensaje")?.Value ?? m.Value;
                    var tipo = m.Element(ns + "tipo")?.Value ?? "";
                    return $"[{tipo}] {codigo} - {mensaje}";
                })
                .ToList() ?? new List<string>();

            var errorMsg = "❌ Comprobante NO AUTORIZADO/DEVUELTO:\n" + string.Join("\n", errorList);
            return (false, errorMsg, null, null);
        }

        return (false, $"⚠️ Estado inesperado: {estado}", null, null);
    }
    catch (Exception ex)
    {
        return (false, $"❌ Error al consultar autorización: {ex.Message}", null, null);
    }
}


}
