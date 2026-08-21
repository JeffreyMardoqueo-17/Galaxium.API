using System.Text.RegularExpressions;
using Galaxium.API.Common;
using Galaxium.API.Entities;
using Galaxium.API.Utils;
using Galaxium.Api.Utils;
using Galaxium.Api.Features.Tenants.Contracts.Requests;
using Galaxium.Api.Features.Tenants.Contracts.Responses;
using Galaxium.Api.Features.Tenants.Repositories;
using Galaxium.Api.Repository.Interfaces;

namespace Galaxium.Api.Features.Tenants.Services;

public class TenantOnboardingService : ITenantOnboardingService
{
    private readonly ITenantOnboardingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TenantOnboardingService(
        ITenantOnboardingRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantOnboardingResponse> OnboardAsync(TenantOnboardingRequest request)
    {
        await ValidateRequestAsync(request);

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var tenant = await CreateTenantAsync(request);
            var role = await CreateAdministratorRoleAsync(tenant.Id);
            var user = await CreateAdministratorUserAsync(request.Administrator, tenant.Id, role.Id);

            return new TenantOnboardingResponse(
                tenant.Id,
                tenant.Name,
                user.Id,
                user.Username,
                "Tenant y administrador creados correctamente.");
        });
    }

    private async Task<Tenant> CreateTenantAsync(TenantOnboardingRequest request)
    {
        var tenant = new Tenant
        {
            Name = request.Tenant.Name.Trim(),
            Slug = request.Tenant.Slug?.Trim().ToLowerInvariant(),
            ContactEmail = request.Tenant.ContactEmail?.Trim(),
            PhoneNumber = request.Tenant.PhoneNumber?.Trim(),
            Address = request.Tenant.Address?.Trim(),
            MaxUsers = request.Tenant.MaxUsers > 0 ? request.Tenant.MaxUsers : 50,
            MaxProducts = request.Tenant.MaxProducts > 0 ? request.Tenant.MaxProducts : 1000
        };

        return await _repository.CreateTenantAsync(tenant);
    }

    private async Task<Role> CreateAdministratorRoleAsync(int tenantId)
    {
        var role = new Role
        {
            TenantId = tenantId,
            Name = GalaxiumRoleNames.Administrator
        };

        return await _repository.CreateRoleAsync(role);
    }

    private async Task<User> CreateAdministratorUserAsync(
        AdministratorData admin, int tenantId, int roleId)
    {
        var user = new User
        {
            TenantId = tenantId,
            RoleId = roleId,
            FullName = admin.FullName.Trim(),
            Username = admin.Username.Trim(),
            Email = admin.Email.Trim(),
            PasswordHash = PasswordHasher.HashPassword(admin.Password)
        };

        return await _repository.CreateUserAsync(user);
    }

    private async Task ValidateRequestAsync(TenantOnboardingRequest request)
    {
        if (request.Tenant == null)
            throw new BusinessException("Los datos del tenant son obligatorios.");

        if (request.Administrator == null)
            throw new BusinessException("Los datos del administrador son obligatorios.");

        if (string.IsNullOrWhiteSpace(request.Tenant.Name))
            throw new BusinessException("El nombre del tenant es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Administrator.FullName))
            throw new BusinessException("El nombre completo del administrador es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Administrator.Username))
            throw new BusinessException("El username del administrador es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Administrator.Email))
            throw new BusinessException("El correo del administrador es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Administrator.Password))
            throw new BusinessException("La contraseña es obligatoria.");

        if (!string.IsNullOrWhiteSpace(request.Tenant.Slug))
        {
            var slugExists = await _repository.SlugExistsAsync(request.Tenant.Slug.Trim());
            if (slugExists)
                throw new BusinessException($"El slug '{request.Tenant.Slug}' ya está en uso.");
        }

        if (!string.IsNullOrWhiteSpace(request.Tenant.ContactEmail))
        {
            var emailExists = await _repository.TenantEmailExistsAsync(request.Tenant.ContactEmail.Trim());
            if (emailExists)
                throw new BusinessException($"El correo '{request.Tenant.ContactEmail}' ya está registrado.");
        }

        ValidatePassword(request.Administrator.Password);
    }

    private static void ValidatePassword(string password)
    {
        if (password.Length < 8)
            throw new BusinessException("La contraseña debe tener al menos 8 caracteres.");

        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new BusinessException("La contraseña debe contener al menos una letra mayúscula.");

        if (!Regex.IsMatch(password, "[a-z]"))
            throw new BusinessException("La contraseña debe contener al menos una letra minúscula.");

        if (!Regex.IsMatch(password, "[0-9]"))
            throw new BusinessException("La contraseña debe contener al menos un número.");

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            throw new BusinessException("La contraseña debe contener al menos un carácter especial.");
    }
}
