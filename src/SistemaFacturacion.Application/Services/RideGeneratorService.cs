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
    private readonly IConfiguracionRepository _configuracionRepo;

    private static class CustomColors
    {
        public static readonly string Primary = "#C8D9C6"; 
        public static readonly string TextDark = "#333333";
        public static readonly string HeaderText = "#555555";
    }

    public RideGeneratorService(
        IFacturaRepository facturaRepo,
        IComprobanteElectronicoRepository comprobanteRepo,
        IConfiguracionRepository configuracionRepo)
    {
        _facturaRepo = facturaRepo;
        _comprobanteRepo = comprobanteRepo;
        _configuracionRepo = configuracionRepo;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerarRidePdfAsync(int idFactura, CancellationToken ct = default)
    {
        var factura = await _facturaRepo.GetByIdAsync(idFactura, includeCliente: true, includeDetalles: true, ct: ct);
        if (factura == null) throw new Exception("Factura no encontrada");

        var comprobante = await _comprobanteRepo.GetByFacturaIdAsync(idFactura, ct);
        if (comprobante == null || string.IsNullOrEmpty(comprobante.XmlAutorizado))
            throw new Exception("No hay comprobante autorizado");

        var config = await _configuracionRepo.GetConfiguracionAsync(ct);
        if (config == null) throw new Exception("No se ha configurado la empresa");

        var qrData = $"{comprobante.ClaveAcceso}\n{factura.FechaEmision:dd/MM/yyyy}\n{comprobante.NumeroAutorizacion}\n{factura.Total:F2}";
        var qrBytes = GenerarCodigoQR(qrData);

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(CustomColors.TextDark));

                page.Header().Element(c => ComposeHeader(c, factura, config));
                page.Content().Element(c => ComposeContent(c, factura, comprobante, qrBytes));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();

        return pdfBytes;
    }

    private void ComposeHeader(IContainer container, Factura factura, ConfiguracionEmpresa config)
    {
        container.Column(column =>
        {
            column.Item().Background(CustomColors.Primary).PaddingVertical(15).Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().Text(config.NombreComercial ?? "TU LOGO AQUÍ").FontSize(14).SemiBold();
                });
            });

            column.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Text("FACTURA").FontSize(36).Bold().FontColor("#555555");

                row.ConstantItem(180).Background("#F0F5F0").Padding(10).Column(col =>
                {
                    col.Item().AlignCenter().Text("FACTURA NÚMERO").FontSize(9).SemiBold();
                    col.Item().AlignCenter().Text(factura.NumeroFactura).FontSize(12);
                });
            });

            column.Item().PaddingTop(20).LineHorizontal(1).LineColor(CustomColors.Primary);

            column.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("DATOS DEL CLIENTE").Bold().FontSize(10);
                    col.Item().PaddingTop(5).Text($"Nombre: {factura.Cliente?.Nombres ?? "Consumidor Final"}");
                    col.Item().Text($"Dirección: {factura.Cliente?.Direccion ?? "S/N"}");
                    col.Item().Text($"RUC/CI: {factura.Cliente?.Identificacion ?? "9999999999"}");
                    col.Item().Text($"Email: {factura.Cliente?.Correo ?? "N/A"}");
                    col.Item().Text($"Teléfono: {factura.Cliente?.Telefono ?? "N/A"}");
                });

                row.RelativeItem().PaddingLeft(20).Column(col =>
                {
                    col.Item().Text(config.RazonSocial ?? "EMPRESA").Bold().FontSize(10);
                    col.Item().PaddingTop(5).Text($"RUC: {config.Ruc}");
                    col.Item().Text($"Dirección: {config.DireccionMatriz}");
                    col.Item().Text($"Email: {config.CorreoEmpresa}");
                });
            });

            column.Item().PaddingTop(15).Row(row =>
            {
                row.AutoItem().Background(CustomColors.Primary).PaddingHorizontal(10).PaddingVertical(5)
                    .Text($"FECHA: {factura.FechaEmision:dd 'de' MMMM 'de' yyyy}");
            });
            
            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(CustomColors.Primary);
        });
    }

    private void ComposeContent(IContainer container, Factura factura, ComprobanteElectronico comprobante, byte[] qrBytes)
    {
        container.PaddingTop(20).Column(column =>
        {
            // TABLA
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); 
                    columns.ConstantColumn(60); 
                    columns.ConstantColumn(80); 
                    columns.ConstantColumn(80); 
                });

                table.Header(header =>
                {
                    header.Cell().Background(CustomColors.Primary).Padding(8).Text("DESCRIPCIÓN").Bold().FontSize(9);
                    header.Cell().Background(CustomColors.Primary).Padding(8).AlignCenter().Text("CANTIDAD").Bold().FontSize(9);
                    header.Cell().Background(CustomColors.Primary).Padding(8).AlignCenter().Text("PRECIO").Bold().FontSize(9);
                    header.Cell().Background(CustomColors.Primary).Padding(8).AlignRight().Text("TOTAL").Bold().FontSize(9);
                });

                foreach (var item in factura.Detalles)
                {
                    table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(8).Text(item.Producto?.Nombre ?? "Item");
                    table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(8).AlignCenter().Text(item.Cantidad.ToString("F0"));
                    table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(8).AlignCenter().Text($"$ {item.PrecioUnitario:F2}");
                    table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(8).AlignRight().Text($"$ {item.TotalLinea:F2}");
                }
            });

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem(); 
                row.ConstantItem(200).Column(col =>
                {
                    col.Item().Row(r => { 
                        r.RelativeItem().Text("Sub Total"); 
                        r.ConstantItem(80).AlignRight().Text($"$ {factura.Subtotal:F2}"); 
                    });
                    col.Item().Row(r => { 
                        r.RelativeItem().Text("IVA 15%"); 
                        r.ConstantItem(80).AlignRight().Text($"$ {factura.Iva:F2}"); 
                    });
                    
                    col.Item().PaddingTop(5).BorderTop(1).BorderColor("#333").PaddingTop(5).Row(r => { 
                        r.RelativeItem().Text("Total").Bold(); 
                        r.ConstantItem(80).AlignRight().Text($"$ {factura.Total:F2}").Bold(); 
                    });
                });
            });

            column.Item().PaddingTop(20).Background("#F0F5F0").Border(1).BorderColor(CustomColors.Primary).Padding(10).Row(row =>
            {
                row.ConstantItem(80).Image(qrBytes);

                row.RelativeItem().PaddingLeft(15).Column(col =>
                {
                    col.Item().Text("INFORMACIÓN TRIBUTARIA").Bold().FontSize(9);
                    col.Item().Text($"Clave de Acceso:").FontSize(8).Bold();
                    col.Item().Text(comprobante.ClaveAcceso ?? "").FontSize(8).FontFamily("Courier New");
                    col.Item().Text($"Autorización: {comprobante.NumeroAutorizacion}").FontSize(8);
                    col.Item().Text("Documento autorizado por el SRI").FontSize(8).Italic();
                });
            });

            column.Item().PaddingTop(20).AlignCenter().Background(CustomColors.Primary).PaddingVertical(8).PaddingHorizontal(20)
                .Text("MUCHAS GRACIAS").FontSize(12).LetterSpacing(2).FontColor("#555555");
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().PaddingTop(10).Text("Generado automáticamente por Sistema Facturación").FontSize(8).FontColor("#999");
    }

    private byte[] GenerarCodigoQR(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}
