using Galaxium.API.Entities;

namespace Galaxium.Api.Entities;

using System.ComponentModel.DataAnnotations;

public class Purchase
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "COMPLETED";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseDetail> Details { get; set; } = new List<PurchaseDetail>();
}
