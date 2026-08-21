using Galaxium.API.Data;
using Galaxium.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Features.Tenants.Repositories;

public class TenantOnboardingRepository : ITenantOnboardingRepository
{
    private readonly GalaxiumDbContext _context;

    public TenantOnboardingRepository(GalaxiumDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        return await _context.Tenant.AnyAsync(t => t.Slug == slug);
    }

    public async Task<bool> TenantEmailExistsAsync(string email)
    {
        return await _context.Tenant.AnyAsync(t => t.ContactEmail == email);
    }

    public async Task<Tenant> CreateTenantAsync(Tenant tenant)
    {
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;
        tenant.IsActive = true;

        _context.Tenant.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        _context.Role.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        _context.User.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
