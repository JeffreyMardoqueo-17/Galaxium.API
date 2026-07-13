using Galaxium.API.Entities;

namespace Galaxium.Api.Features.Tenants.Services;

public interface ITenantService
{
    Task<Tenant?> GetByIdAsync(int tenantId);
    Task<Tenant?> GetBySlugAsync(string slug);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Tenant> CreateAsync(string name, string? slug, string? contactEmail, string? phoneNumber, string? address, int maxUsers, int maxProducts);
    Task<Tenant?> UpdateAsync(int tenantId, string name, string? slug, string? contactEmail, string? phoneNumber, string? address, string? logoUrl, bool isActive, int maxUsers, int maxProducts);
}
