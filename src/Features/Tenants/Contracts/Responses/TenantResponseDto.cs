namespace Galaxium.Api.Features.Tenants.Contracts.Responses;

public record TenantResponseDto(
    int Id,
    string Name,
    string? Slug,
    string? ContactEmail,
    string? PhoneNumber,
    string? Address,
    string? LogoUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SubscriptionExpiresAt,
    int MaxUsers,
    int MaxProducts
);
