namespace Galaxium.Api.DTOs.Supplier;

public record SupplierResponseDto(
    int Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    bool IsActive,
    DateTime CreatedAt
);
