
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Galaxium.API.Entities
{
    public class ProductCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        // CLAVE PARA SKU
        [MaxLength(20)]
        public string? Code { get; set; }// HIG, ELE, TEC

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}