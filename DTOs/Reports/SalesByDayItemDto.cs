namespace Galaxium.Api.DTOs.Reports;

public record SalesByDayItemDto(
    DateTime Date,
    int Transactions,
    decimal TotalAmount
);
