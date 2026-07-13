
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Galaxium.Api.Shared.MultiTenant;
using Galaxium.API.Entities;

namespace Galaxium.Api.Entities
{
    public class PaymentMethod : ITenantEntity
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
    }
}