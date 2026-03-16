using Galaxium.Api.DTOs.Reports;

namespace Galaxium.Api.Services.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<SalesByDayItemDto>> GetSalesByDayAsync(DateTime? startDate, DateTime? endDate);
    Task<IReadOnlyList<SalesByProductItemDto>> GetSalesByProductAsync(DateTime? startDate, DateTime? endDate);
    Task<IReadOnlyList<SalesByCategoryItemDto>> GetSalesByCategoryAsync(DateTime? startDate, DateTime? endDate);
    Task<ProfitSummaryDto> GetProfitSummaryAsync(DateTime? startDate, DateTime? endDate);
    Task<IReadOnlyList<InventorySnapshotItemDto>> GetInventorySnapshotAsync();
    Task<IReadOnlyList<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(DateTime? startDate, DateTime? endDate);
}
