using System;
using System.Collections.Generic;

namespace Galaxium.API.Entities
{
    public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int CreatedByUserId { get; set; }

    public string Name { get; set; } = null!;
    public string SKU { get; set; } = null!;

    // 👇 CLAVE
    public decimal? CostPrice { get; set; }
    public decimal? SalePrice { get; set; }

    public int Stock { get; set; }
    public int MinimumStock { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navegaciones
    public ProductCategory Category { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
}

}
