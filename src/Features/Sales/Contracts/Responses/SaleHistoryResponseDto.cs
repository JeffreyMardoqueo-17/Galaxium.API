namespace Galaxium.API.DTOs;

public record SaleHistoryResponseDto(
    DateTime StartDate,
    DateTime EndDate,
    SaleHistorySummaryDto Summary,
    IReadOnlyCollection<SaleHistoryItemDto> Sales
);
