using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Galaxium.Api.Entities;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.API.Entities
{
    public class Tenant : ITenantEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(150)]
        public string? Slug { get; set; }

        [MaxLength(150)]
        public string? ContactEmail { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? SubscriptionExpiresAt { get; set; }

        public int MaxUsers { get; set; } = 50;

        public int MaxProducts { get; set; } = 1000;

        // TenantId for ITenantEntity - always equals Id for the Tenant itself
        public int TenantId { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
