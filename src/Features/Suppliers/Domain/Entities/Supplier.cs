using Galaxium.Api.Entities;
using Galaxium.Api.Shared.MultiTenant;
using Galaxium.API.Entities;

using System.ComponentModel.DataAnnotations;

public class Supplier : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
}
