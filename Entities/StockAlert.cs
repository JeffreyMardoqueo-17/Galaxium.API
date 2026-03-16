using Galaxium.Api.Enums;
using Galaxium.API.Entities;

namespace Galaxium.Api.Entities;

public class StockAlert
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public StockAlertType AlertType { get; set; }
    public string Message { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
