using System;
using System.Collections.Generic;
using Galaxium.Api.Entities;

namespace Galaxium.API.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int CreatedByUserId { get; set; }

        public string Name { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public string? Barcode { get; set; }

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
        public ICollection<ProductPhoto> Photos { get; set; } = new List<ProductPhoto>();
        public ICollection<StockEntry> StockEntries { get; set; }
    = new List<StockEntry>();


    }

}
