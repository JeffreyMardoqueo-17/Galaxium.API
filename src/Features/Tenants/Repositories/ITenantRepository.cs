using Galaxium.API.Entities;

namespace Galaxium.Api.Features.Tenants.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(int tenantId);
    Task<Tenant?> GetBySlugAsync(string slug);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Tenant> CreateAsync(Tenant tenant);
    Task<Tenant?> UpdateAsync(Tenant tenant);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
}
