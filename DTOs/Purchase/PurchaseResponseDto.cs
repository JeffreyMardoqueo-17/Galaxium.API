namespace Galaxium.Api.DTOs.Purchase;

public record PurchaseResponseDto(
    int Id,
    int SupplierId,
    string SupplierName,
    int UserId,
    DateTime PurchaseDate,
    decimal Total,
    string Status,
    IReadOnlyList<PurchaseDetailResponseDto> Details
);
