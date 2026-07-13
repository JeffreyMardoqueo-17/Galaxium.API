using Galaxium.API.Entities;
using Galaxium.Api.Features.Tenants.Repositories;

namespace Galaxium.Api.Features.Tenants.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;

    public TenantService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Tenant?> GetByIdAsync(int tenantId)
    {
        return await _tenantRepository.GetByIdAsync(tenantId);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("El slug no puede estar vacío.");

        return await _tenantRepository.GetBySlugAsync(slug);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _tenantRepository.GetAllAsync();
    }

    public async Task<Tenant> CreateAsync(
        string name, string? slug, string? contactEmail,
        string? phoneNumber, string? address, int maxUsers, int maxProducts)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del tenant es obligatorio.");

        if (await _tenantRepository.ExistsByNameAsync(name))
            throw new InvalidOperationException($"Ya existe un tenant con el nombre '{name}'.");

        var tenant = new Tenant
        {
            Name = name.Trim(),
            Slug = slug?.Trim().ToLowerInvariant(),
            ContactEmail = contactEmail?.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            Address = address?.Trim(),
            MaxUsers = maxUsers > 0 ? maxUsers : 50,
            MaxProducts = maxProducts > 0 ? maxProducts : 1000
        };

        return await _tenantRepository.CreateAsync(tenant);
    }

    public async Task<Tenant?> UpdateAsync(
        int tenantId, string name, string? slug, string? contactEmail,
        string? phoneNumber, string? address, string? logoUrl,
        bool isActive, int maxUsers, int maxProducts)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del tenant es obligatorio.");

        if (await _tenantRepository.ExistsByNameAsync(name, tenantId))
            throw new InvalidOperationException($"Ya existe otro tenant con el nombre '{name}'.");

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = name.Trim(),
            Slug = slug?.Trim().ToLowerInvariant(),
            ContactEmail = contactEmail?.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            Address = address?.Trim(),
            LogoUrl = logoUrl,
            IsActive = isActive,
            MaxUsers = maxUsers > 0 ? maxUsers : 50,
            MaxProducts = maxProducts > 0 ? maxProducts : 1000
        };

        return await _tenantRepository.UpdateAsync(tenant);
    }
}
