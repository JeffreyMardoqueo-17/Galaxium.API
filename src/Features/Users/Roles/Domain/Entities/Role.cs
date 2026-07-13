
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.API.Entities
{
    public class Role : ITenantEntity
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        public Tenant Tenant { get; set; } = null!;
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
