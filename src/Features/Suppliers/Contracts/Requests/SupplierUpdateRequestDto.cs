namespace Galaxium.Api.DTOs.Supplier;

public record SupplierUpdateRequestDto(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    bool IsActive
);
