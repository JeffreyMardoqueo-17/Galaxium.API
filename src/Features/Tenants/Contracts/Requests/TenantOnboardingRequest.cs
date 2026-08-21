using System.ComponentModel.DataAnnotations;

namespace Galaxium.Api.Features.Tenants.Contracts.Requests;

public class TenantOnboardingRequest
{
    [Required]
    public TenantData Tenant { get; set; } = null!;

    [Required]
    public AdministratorData Administrator { get; set; } = null!;
}

public class TenantData
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(150)]
    public string? Slug { get; set; }

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public int MaxUsers { get; set; }
    public int MaxProducts { get; set; }
}

public class AdministratorData
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}
