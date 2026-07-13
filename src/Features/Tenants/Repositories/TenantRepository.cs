using Galaxium.API.Data;
using Galaxium.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Features.Tenants.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly GalaxiumDbContext _context;

    public TenantRepository(GalaxiumDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(int tenantId)
    {
        return await _context.Tenant
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug)
    {
        return await _context.Tenant
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _context.Tenant
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;
        tenant.IsActive = true;

        _context.Tenant.Add(tenant);
        await _context.SaveChangesAsync();

        return tenant;
    }

    public async Task<Tenant?> UpdateAsync(Tenant tenant)
    {
        var existing = await _context.Tenant.FirstOrDefaultAsync(t => t.Id == tenant.Id);
        if (existing == null) return null;

        existing.Name = tenant.Name;
        existing.Slug = tenant.Slug;
        existing.ContactEmail = tenant.ContactEmail;
        existing.PhoneNumber = tenant.PhoneNumber;
        existing.Address = tenant.Address;
        existing.LogoUrl = tenant.LogoUrl;
        existing.IsActive = tenant.IsActive;
        existing.MaxUsers = tenant.MaxUsers;
        existing.MaxProducts = tenant.MaxProducts;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        return await _context.Tenant
            .AnyAsync(t => t.Name == name && (!excludeId.HasValue || t.Id != excludeId.Value));
    }
}
