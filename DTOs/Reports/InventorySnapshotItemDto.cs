namespace Galaxium.Api.DTOs.Reports;

public record InventorySnapshotItemDto(
    int ProductId,
    string ProductName,
    string SKU,
    int Stock,
    int MinimumStock,
    bool IsLowStock,
    bool IsExhausted
);
