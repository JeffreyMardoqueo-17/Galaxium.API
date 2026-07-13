
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.API.Entities
{
    public class Customer : ITenantEntity
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        // Nombre completo del cliente
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        // Teléfono de contacto
        [MaxLength(30)]
        public string? Phone { get; set; }

        // Correo electrónico
        [MaxLength(150)]
        public string? Email { get; set; }

        // Fecha de registro
        public DateTime CreatedAt { get; set; }

        // Navegación
        public Tenant Tenant { get; set; } = null!;
        // Ventas realizadas por el cliente
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }

}