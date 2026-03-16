namespace Galaxium.Api.DTOs.Reports;

public record SalesByProductItemDto(
    int ProductId,
    string ProductName,
    int QuantitySold,
    decimal TotalAmount
);
