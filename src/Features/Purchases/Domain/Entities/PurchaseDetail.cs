using Galaxium.API.Entities;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.Api.Entities;

public class PurchaseDetail : ITenantEntity
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int TenantId { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}
