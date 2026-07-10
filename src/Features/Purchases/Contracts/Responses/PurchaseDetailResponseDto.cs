namespace Galaxium.Api.DTOs.Purchase;

public record PurchaseDetailResponseDto(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Total
);
