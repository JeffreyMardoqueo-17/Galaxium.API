using System.Globalization;
using System.IO;
using Galaxium.API.Entities;
using Galaxium.Api.Services.Interfaces;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Galaxium.Api.Services.Implementations;

public class SalePdfService : ISalePdfService
{
    private static readonly TimeZoneInfo ElSalvadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

    public byte[] GenerateInvoicePdf(Sale sale)
    {
        using var stream = new MemoryStream();
        using var document = new PdfDocument();
        document.Info.Title = $"Factura {sale.InvoiceNumber}";

        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
        var subtitleFont = new XFont("Arial", 10, XFontStyle.Regular);
        var headerFont = new XFont("Arial", 11, XFontStyle.Bold);
        var textFont = new XFont("Arial", 10, XFontStyle.Regular);

        double y = 36;
        DrawText(gfx, "Galaxium ERP", titleFont, 40, ref y);
        DrawText(gfx, $"Factura: {sale.InvoiceNumber ?? $"VENTA-{sale.Id}"}", subtitleFont, 40, ref y);
        var localSaleDate = TimeZoneInfo.ConvertTimeFromUtc(sale.SaleDate, ElSalvadorTimeZone);
        DrawText(gfx, $"Fecha: {localSaleDate:dd/MM/yyyy HH:mm}", subtitleFont, 40, ref y);
        DrawText(gfx, $"Cliente: {sale.Customer?.FullName ?? "Consumidor final"}", subtitleFont, 40, ref y);
        DrawText(gfx, $"Vendedor: {sale.User?.FullName ?? "Usuario interno"}", subtitleFont, 40, ref y);
        DrawText(gfx, $"Metodo de pago: {sale.PaymentMethod?.Name ?? "No definido"}", subtitleFont, 40, ref y);

        y += 8;
        DrawText(gfx, "Detalle de productos", headerFont, 40, ref y);

        y += 4;
        DrawText(gfx, "Producto", headerFont, 40, ref y);
        DrawText(gfx, "Cant", headerFont, 290, y - 14);
        DrawText(gfx, "P.Unit", headerFont, 340, y - 14);
        DrawText(gfx, "SubTotal", headerFont, 430, y - 14);

        y += 2;
        gfx.DrawLine(XPens.LightGray, 40, y, 550, y);
        y += 10;

        foreach (var detail in sale.Details)
        {
            EnsurePageSpace(document, ref page, ref gfx, ref y, 90);

            var productName = detail.Product?.Name ?? $"Producto #{detail.ProductId}";
            DrawText(gfx, Truncate(productName, 36), textFont, 40, ref y, 14);
            DrawText(gfx, detail.Quantity.ToString(CultureInfo.InvariantCulture), textFont, 290, y - 14);
            DrawText(gfx, detail.UnitPrice.ToString("C2", CultureInfo.GetCultureInfo("es-SV")), textFont, 340, y - 14);
            DrawText(gfx, (detail.Quantity * detail.UnitPrice).ToString("C2", CultureInfo.GetCultureInfo("es-SV")), textFont, 430, y - 14);
        }

        y += 8;
        gfx.DrawLine(XPens.Gray, 320, y, 550, y);
        y += 16;

        DrawAmountLine(gfx, textFont, "Subtotal:", sale.SubTotal, ref y);
        DrawAmountLine(gfx, textFont, "Descuento:", sale.Discount, ref y);
        DrawAmountLine(gfx, headerFont, "Total:", sale.Total, ref y);
        DrawAmountLine(gfx, textFont, "Monto recibido:", sale.AmountPaid, ref y);
        DrawAmountLine(gfx, textFont, "Vuelto:", sale.ChangeAmount, ref y);

        y += 12;
        DrawText(gfx, "Gracias por tu compra.", subtitleFont, 40, ref y);

        document.Save(stream, false);
        return stream.ToArray();
    }

    public byte[] GenerateSalesReportPdf(
        IReadOnlyCollection<Sale> sales,
        DateTime startDate,
        DateTime endDate,
        string title)
    {
        using var stream = new MemoryStream();
        using var document = new PdfDocument();
        document.Info.Title = title;

        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
        var subtitleFont = new XFont("Arial", 10, XFontStyle.Regular);
        var headerFont = new XFont("Arial", 9, XFontStyle.Bold);
        var textFont = new XFont("Arial", 9, XFontStyle.Regular);

        var culture = CultureInfo.GetCultureInfo("es-SV");

        double y = 34;
        DrawText(gfx, title, titleFont, 40, ref y);
        DrawText(gfx, $"Rango: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}", subtitleFont, 40, ref y);
        DrawText(gfx, $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", subtitleFont, 40, ref y);

        y += 8;
        var totalProducts = sales.SelectMany(s => s.Details).Sum(d => d.Quantity);
        var totalRevenue = sales.Sum(s => s.Total);
        var totalDiscount = sales.Sum(s => s.Discount);
        var totalAmountPaid = sales.Sum(s => s.AmountPaid);

        DrawText(gfx, $"Ventas registradas: {sales.Count}", subtitleFont, 40, ref y);
        DrawText(gfx, $"Productos vendidos: {totalProducts}", subtitleFont, 40, ref y);
        DrawText(gfx, $"Total facturado: {totalRevenue.ToString("C2", culture)}", subtitleFont, 40, ref y);
        DrawText(gfx, $"Descuento acumulado: {totalDiscount.ToString("C2", culture)}", subtitleFont, 40, ref y);
        DrawText(gfx, $"Monto recibido: {totalAmountPaid.ToString("C2", culture)}", subtitleFont, 40, ref y);

        y += 10;
        DrawText(gfx, "Factura", headerFont, 40, ref y);
        DrawText(gfx, "Fecha", headerFont, 130, y - 14);
        DrawText(gfx, "Cliente", headerFont, 200, y - 14);
        DrawText(gfx, "Metodo", headerFont, 320, y - 14);
        DrawText(gfx, "Productos", headerFont, 410, y - 14);
        DrawText(gfx, "Total", headerFont, 480, y - 14);

        y += 2;
        gfx.DrawLine(XPens.LightGray, 40, y, 550, y);
        y += 10;

        foreach (var sale in sales)
        {
            EnsurePageSpace(document, ref page, ref gfx, ref y, 80);

            var productsSold = sale.Details?.Sum(d => d.Quantity) ?? 0;
            DrawText(gfx, Truncate(sale.InvoiceNumber ?? $"VENTA-{sale.Id}", 15), textFont, 40, ref y, 13);
            var localReportDate = TimeZoneInfo.ConvertTimeFromUtc(sale.SaleDate, ElSalvadorTimeZone);
            DrawText(gfx, localReportDate.ToString("dd/MM/yyyy", culture), textFont, 130, y - 13);
            DrawText(gfx, Truncate(sale.Customer?.FullName ?? "Consumidor final", 20), textFont, 200, y - 13);
            DrawText(gfx, Truncate(sale.PaymentMethod?.Name ?? "N/A", 12), textFont, 320, y - 13);
            DrawText(gfx, productsSold.ToString(CultureInfo.InvariantCulture), textFont, 430, y - 13);
            DrawText(gfx, sale.Total.ToString("C2", culture), textFont, 480, y - 13);
        }

        document.Save(stream, false);
        return stream.ToArray();
    }

    private static void DrawText(XGraphics gfx, string text, XFont font, double x, ref double y, double lineHeight = 16)
    {
        gfx.DrawString(text, font, XBrushes.Black, new XPoint(x, y));
        y += lineHeight;
    }

    private static void DrawText(XGraphics gfx, string text, XFont font, double x, double y)
    {
        gfx.DrawString(text, font, XBrushes.Black, new XPoint(x, y));
    }

    private static void DrawAmountLine(XGraphics gfx, XFont font, string label, decimal value, ref double y)
    {
        var culture = CultureInfo.GetCultureInfo("es-SV");
        DrawText(gfx, label, font, 340, y);
        DrawText(gfx, value.ToString("C2", culture), font, 470, y);
        y += 16;
    }

    private static string Truncate(string value, int max)
    {
        return value.Length <= max ? value : value[..(max - 3)] + "...";
    }

    private static void EnsurePageSpace(
        PdfDocument document,
        ref PdfPage page,
        ref XGraphics gfx,
        ref double y,
        double bottomPadding)
    {
        if (y <= page.Height - bottomPadding)
        {
            return;
        }

        page = document.AddPage();
        gfx = XGraphics.FromPdfPage(page);
        y = 34;
    }
}
