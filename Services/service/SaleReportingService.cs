using Galaxium.API.DTOs;
using Galaxium.API.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;

namespace Galaxium.Api.Services.Implementations;

public class SaleReportingService : ISaleReportingService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISalePdfService _salePdfService;

    public SaleReportingService(
        ISaleRepository saleRepository,
        ISalePdfService salePdfService)
    {
        _saleRepository = saleRepository;
        _salePdfService = salePdfService;
    }

    public async Task<SaleHistoryResponseDto> GetSalesHistoryAsync(DateTime? startDate, DateTime? endDate)
    {
        var (rangeStart, rangeEnd) = NormalizeDateRange(startDate, endDate);
        var sales = (await _saleRepository.GetByDateRangeAsync(rangeStart, rangeEnd)).ToList();

        var summary = BuildSummary(sales);
        var items = sales.Select(MapItem).ToList();

        return new SaleHistoryResponseDto(rangeStart, rangeEnd, summary, items);
    }

    public async Task<(byte[] Content, string FileName)> GenerateInvoicePdfAsync(int saleId)
    {
        if (saleId <= 0)
        {
            throw new ArgumentException("El id de venta debe ser mayor a cero.");
        }

        var sale = await _saleRepository.GetByIdAsync(saleId)
            ?? throw new InvalidOperationException($"No se encontró la venta con Id {saleId}.");

        var content = _salePdfService.GenerateInvoicePdf(sale);
        var invoiceNumber = string.IsNullOrWhiteSpace(sale.InvoiceNumber)
            ? $"VENTA-{sale.Id}"
            : sale.InvoiceNumber;

        return (content, $"Factura-{invoiceNumber}.pdf");
    }

    public async Task<(byte[] Content, string FileName)> GenerateSalesReportPdfAsync(DateTime? startDate, DateTime? endDate)
    {
        var (rangeStart, rangeEnd) = NormalizeDateRange(startDate, endDate);
        var sales = (await _saleRepository.GetByDateRangeAsync(rangeStart, rangeEnd)).ToList();

        var content = _salePdfService.GenerateSalesReportPdf(
            sales,
            rangeStart,
            rangeEnd,
            "Reporte monetario de ventas");

        return (content, $"Reporte-Ventas-{rangeStart:yyyyMMdd}-{rangeEnd:yyyyMMdd}.pdf");
    }

    public async Task<(byte[] Content, string FileName)> GenerateDailyInvoicesPdfAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

        var sales = (await _saleRepository.GetByDateRangeAsync(dayStart, dayEnd)).ToList();
        var content = _salePdfService.GenerateSalesReportPdf(
            sales,
            dayStart,
            dayEnd,
            "Facturas del día");

        return (content, $"Facturas-Dia-{dayStart:yyyyMMdd}.pdf");
    }

    private static (DateTime Start, DateTime End) NormalizeDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue && !endDate.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            return (today.AddDays(-30), today.AddDays(1).AddTicks(-1));
        }

        var start = (startDate ?? endDate!.Value).Date;
        var end = (endDate ?? startDate!.Value).Date.AddDays(1).AddTicks(-1);

        if (start > end)
        {
            throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha final.");
        }

        if ((end - start).TotalDays > 3660)
        {
            throw new InvalidOperationException("El rango permitido no puede superar 10 años.");
        }

        return (start, end);
    }

    private static SaleHistorySummaryDto BuildSummary(IReadOnlyCollection<Sale> sales)
    {
        var totalSales = sales.Count;
        var totalProductsSold = sales.SelectMany(s => s.Details).Sum(d => d.Quantity);
        var totalSubTotal = sales.Sum(s => s.SubTotal);
        var totalDiscount = sales.Sum(s => s.Discount);
        var totalRevenue = sales.Sum(s => s.Total);
        var totalAmountPaid = sales.Sum(s => s.AmountPaid);
        var totalChangeDelivered = sales.Sum(s => s.ChangeAmount);
        var averageTicket = totalSales > 0 ? totalRevenue / totalSales : 0m;

        return new SaleHistorySummaryDto(
            totalSales,
            totalProductsSold,
            totalSubTotal,
            totalDiscount,
            totalRevenue,
            totalAmountPaid,
            totalChangeDelivered,
            averageTicket);
    }

    private static SaleHistoryItemDto MapItem(Sale sale)
    {
        var productsSold = sale.Details?.Sum(d => d.Quantity) ?? 0;

        return new SaleHistoryItemDto(
            sale.Id,
            sale.InvoiceNumber ?? $"VENTA-{sale.Id}",
            sale.SaleDate,
            sale.Customer?.FullName ?? "Consumidor final",
            sale.User?.FullName ?? "Usuario interno",
            sale.PaymentMethod?.Name ?? "No definido",
            productsSold,
            sale.SubTotal,
            sale.Discount,
            sale.Total,
            sale.AmountPaid,
            sale.ChangeAmount);
    }
}
