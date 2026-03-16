namespace Galaxium.Api.DTOs.Supplier;

public record SupplierCreateRequestDto(
    string Name,
    string? Phone,
    string? Email,
    string? Address
);
