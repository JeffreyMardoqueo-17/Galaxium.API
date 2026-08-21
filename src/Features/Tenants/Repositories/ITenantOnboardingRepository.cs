using Galaxium.API.Entities;

namespace Galaxium.Api.Features.Tenants.Repositories;

public interface ITenantOnboardingRepository
{
    Task<bool> SlugExistsAsync(string slug);
    Task<bool> TenantEmailExistsAsync(string email);
    Task<Tenant> CreateTenantAsync(Tenant tenant);
    Task<Role> CreateRoleAsync(Role role);
    Task<User> CreateUserAsync(User user);
}
