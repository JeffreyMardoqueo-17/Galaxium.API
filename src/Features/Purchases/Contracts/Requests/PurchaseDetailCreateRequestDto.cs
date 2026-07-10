namespace Galaxium.Api.DTOs.Purchase;

public record PurchaseDetailCreateRequestDto(
    int ProductId,
    int Quantity,
    decimal UnitPrice
);
