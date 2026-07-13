using Galaxium.Api.Enums;
using Galaxium.API.Entities;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.Api.Entities;

using System.ComponentModel.DataAnnotations;

public class StockAlert : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public StockAlertType AlertType { get; set; }

    [Required]
    [MaxLength(300)]
    public string Message { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
