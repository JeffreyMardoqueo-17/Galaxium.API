namespace Galaxium.Api.Features.Tenants.Contracts.Requests;

public record TenantCreateRequest(
    string Name,
    string? Slug,
    string? ContactEmail,
    string? PhoneNumber,
    string? Address,
    int MaxUsers = 50,
    int MaxProducts = 1000
);

public record TenantUpdateRequest(
    string Name,
    string? Slug,
    string? ContactEmail,
    string? PhoneNumber,
    string? Address,
    string? LogoUrl,
    bool IsActive,
    int MaxUsers,
    int MaxProducts
);
