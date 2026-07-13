
using System;
using System.ComponentModel.DataAnnotations;
using Galaxium.API.Entities;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.Api.Entities
{
    public class PasswordResetCode : ITenantEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }

        /// <summary>SHA-256 hash del código de 6 dígitos.</summary>
        [Required]
        [MaxLength(100)]
        public string CodeHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navegación
        public User User { get; set; } = null!;
    }
}
