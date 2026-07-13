
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.API.Entities
{
    public class ProductCategory : ITenantEntity
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        // CLAVE PARA SKU
        [MaxLength(20)]
        public string? Code { get; set; }// HIG, ELE, TEC

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public Tenant Tenant { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}