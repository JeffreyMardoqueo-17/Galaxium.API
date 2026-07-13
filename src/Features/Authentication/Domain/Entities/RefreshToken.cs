
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.API.Entities
{
   public class RefreshToken : ITenantEntity
   {
       public int Id { get; set; }
       public int UserId { get; set; }
       public int TenantId { get; set; }

       [Required]
       [MaxLength(300)]
       public string Token { get; set; } = string.Empty;

       public DateTime ExpiresAt { get; set; }
       public bool IsRevoked { get; set; }
       public DateTime CreatedAt { get; set; }
       public DateTime? RevokedAt { get; set; }

       [MaxLength(300)]
       public string? ReplacedByToken { get; set; }

       public User User { get; set; } = null!;
    }
}

