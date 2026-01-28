using System;
using System.Collections.Generic;

namespace Galaxium.API.Entities
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // 🔹 Navegación
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
