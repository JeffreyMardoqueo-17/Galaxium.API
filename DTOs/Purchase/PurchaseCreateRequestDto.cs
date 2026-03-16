namespace Galaxium.Api.DTOs.Purchase;

public record PurchaseCreateRequestDto(
    int SupplierId,
    List<PurchaseDetailCreateRequestDto> Details
);
