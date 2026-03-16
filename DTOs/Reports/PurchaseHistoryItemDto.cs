namespace Galaxium.Api.DTOs.Reports;

public record PurchaseHistoryItemDto(
    int PurchaseId,
    DateTime PurchaseDate,
    int SupplierId,
    string SupplierName,
    decimal Total,
    int Lines
);
