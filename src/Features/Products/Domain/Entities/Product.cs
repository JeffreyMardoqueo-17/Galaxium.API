
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.API.Entities
{
    public class Product : ITenantEntity
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int CategoryId { get; set; }
        public int CreatedByUserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = null!;

        [MaxLength(100)]
        public string? Barcode { get; set; }

        // 👇 CLAVE
        public decimal? CostPrice { get; set; }
        public decimal? SalePrice { get; set; }

        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Unit;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navegaciones
        public Tenant Tenant { get; set; } = null!;
        public ProductCategory Category { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
        public ICollection<ProductPhoto> Photos { get; set; } = new List<ProductPhoto>();
        public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
        public ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();
    }

}
