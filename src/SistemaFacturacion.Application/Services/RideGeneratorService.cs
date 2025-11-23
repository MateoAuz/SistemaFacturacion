using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using System.Xml.Linq;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Application.Contracts;

namespace SistemaFacturacion.Application.Services;

public interface IRideGeneratorService
{
    Task<byte[]> GenerarRidePdfAsync(int idFactura, CancellationToken ct = default);
}

public class RideGeneratorService : IRideGeneratorService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IComprobanteElectronicoRepository _comprobanteRepo;

    public RideGeneratorService(
        IFacturaRepository facturaRepo,
        IComprobanteElectronicoRepository comprobanteRepo)
    {
        _facturaRepo = facturaRepo;
        _comprobanteRepo = comprobanteRepo;
        
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerarRidePdfAsync(int idFactura, CancellationToken ct = default)
    {
        // ✅ CORRECCIÓN 1: Llama al repositorio incluyendo las entidades relacionadas
        var factura = await _facturaRepo.GetByIdAsync(idFactura, includeCliente: true, includeDetalles: true, ct: ct);
        if (factura == null)
            throw new Exception("Factura no encontrada");

        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);
        if (comprobante == null || string.IsNullOrEmpty(comprobante.XmlAutorizado))
            throw new Exception("No hay comprobante electrónico autorizado");

        if (comprobante.EstadoEnvio != "AUTORIZADO")
            throw new Exception("El comprobante no está autorizado por el SRI");

        // Parsear XML autorizado
        var xmlDoc = XDocument.Parse(comprobante.XmlAutorizado);

        // Generar código QR
        var qrData = $"{comprobante.ClaveAcceso}\n" +
                     $"{factura.FechaEmision:dd/MM/yyyy}\n" +
                     $"{comprobante.NumeroAutorizacion}\n" +
                     $"{factura.Total:F2}";
        var qrBytes = GenerarCodigoQR(qrData);

        // Generar PDF
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                
                page.Header().Element(c => ComposeHeader(c, factura, comprobante));
                page.Content().Element(c => ComposeContent(c, factura));
                page.Footer().Element(c => ComposeFooter(c, comprobante, qrBytes));
            });
        }).GeneratePdf();

        return pdfBytes;
    }

    private void ComposeHeader(IContainer container, Factura factura, ComprobanteElectronico comprobante)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("TU EMPRESA S.A.").Bold().FontSize(14);
                    col.Item().Text("RUC: 1234567890001").FontSize(10);
                    col.Item().Text("Dirección Matriz: Av. Principal 123").FontSize(9);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text("FACTURA").Bold().FontSize(16);
                    col.Item().AlignRight().Text($"No. {factura.NumeroFactura}").FontSize(11);
                    col.Item().AlignRight().PaddingTop(5).Text("NÚMERO DE AUTORIZACIÓN").Bold().FontSize(8);
                    col.Item().AlignRight().Text(comprobante.NumeroAutorizacion ?? "").FontSize(7);
                });
            });

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            column.Item().Column(col =>
            {
                col.Item().Text("DATOS DEL CLIENTE").Bold().FontSize(10);
                col.Item().PaddingTop(5).Row(row =>
                {
                    // ✅ CORRECCIÓN 2: Asumo que la propiedad en Cliente se llama 'Nombres' y 'Cedula'
                    // Verifica el nombre real en tu clase 'Cliente.cs'
                    row.RelativeItem().Text($"Razón Social: {factura.Cliente?.Nombres ?? "Consumidor Final"}").FontSize(9);
                    row.AutoItem().Width(150).Text($"RUC/CI: {factura.Cliente?.Identificacion ?? "9999999999"}").FontSize(9);
                });
                 // ✅ CORRECCIÓN 3: Asumo que la propiedad en Cliente se llama 'Correo'
                 // Verifica el nombre real en tu clase 'Cliente.cs'
                col.Item().Text($"Dirección: {factura.Cliente?.Direccion ?? "N/A"}").FontSize(9);
                col.Item().Text($"Email: {factura.Cliente?.Correo ?? "N/A"}").FontSize(9);
            });

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(IContainer container, Factura factura)
    {
        container.PaddingTop(10).Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Cant.").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Descripción").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("P. Unit.").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Subtotal").Bold().FontColor(Colors.White);
                });

                foreach (var detalle in factura.Detalles)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(detalle.Cantidad.ToString("F2"));
                    
                    // ✅ CORRECCIÓN 4: Asumo que DetalleFactura tiene navegación a Producto.Nombre
                    // Verifica tu clase 'DetalleFactura.cs' y 'Producto.cs'
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(detalle.Producto?.Nombre ?? "Ítem sin descripción");
                    
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .AlignRight().Text($"${detalle.PrecioUnitario:F2}");
                    
                    // ✅ CORRECCIÓN 5: Asumo que DetalleFactura tiene una propiedad 'Total'
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .AlignRight().Text($"${detalle.TotalLinea:F2}");
                }
            });

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem();
                row.ConstantItem(200).Column(col =>
                {
                    col.Item().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(5);
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("SUBTOTAL:");
                        r.ConstantItem(80).AlignRight().Text($"${factura.Subtotal:F2}");
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("IVA 15%:");
                        r.ConstantItem(80).AlignRight().Text($"${factura.Iva:F2}");
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL:").Bold();
                        r.ConstantItem(80).AlignRight().Text($"${factura.Total:F2}").Bold();
                    });
                });
            });
            
            column.Item().PaddingTop(15).Column(col =>
            {
                col.Item().Text("INFORMACIÓN ADICIONAL").Bold().FontSize(10);
                // ✅ CORRECCIÓN 6: Extrae la forma de pago desde la colección de Pagos
                var formaPago = factura.Pagos?.FirstOrDefault()?.MetodoPago ?? "SIN UTILIZACION DEL SISTEMA FINANCIERO";
                col.Item().PaddingTop(3).Text($"Forma de pago: {formaPago}").FontSize(9);
            });
        });
    }

    private void ComposeFooter(IContainer container, ComprobanteElectronico comprobante, byte[] qrBytes)
    {
         container.Column(column =>
        {
            column.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Medium);
            column.Item().PaddingTop(10).Row(row =>
            {
                row.ConstantItem(100).Image(qrBytes);
                row.RelativeItem().PaddingLeft(10).Column(col =>
                {
                    col.Item().Text("CLAVE DE ACCESO").Bold().FontSize(9);
                    col.Item().Text(comprobante.ClaveAcceso ?? "").FontSize(8);
                });
            });
        });
    }

    private byte[] GenerarCodigoQR(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}
