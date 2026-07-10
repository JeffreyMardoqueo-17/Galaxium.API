namespace Galaxium.Api.DTOs.Reports;

public record SalesByCategoryItemDto(
    int CategoryId,
    string CategoryName,
    int QuantitySold,
    decimal TotalAmount
);
