using System;
using System.Collections.Generic;

namespace Galaxium.API.Entities
{
    public class ProductCategory
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        // CLAVE PARA SKU
        public string Code { get; set; } = null!; // HIG, ELE, TEC

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}