using Galaxium.API.DTOs;

namespace Galaxium.Api.Services.Interfaces;

public interface ISaleReportingService
{
    Task<SaleHistoryResponseDto> GetSalesHistoryAsync(DateTime? startDate, DateTime? endDate);
    Task<(byte[] Content, string FileName)> GenerateInvoicePdfAsync(int saleId);
    Task<(byte[] Content, string FileName)> GenerateSalesReportPdfAsync(DateTime? startDate, DateTime? endDate);
    Task<(byte[] Content, string FileName)> GenerateDailyInvoicesPdfAsync(DateTime date);
}
