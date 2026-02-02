using System;

namespace Galaxium.API.Models
{
    public class ProductFilterModel
    {
        // 🎯 Filtros
        public int? CategoryId { get; set; }
        public string? Name { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }

        public bool? IsActive { get; set; }

        // 🔽 Ordenamiento
        public string OrderBy { get; set; } = "CreatedAt";
        public bool OrderDescending { get; set; } = true;

        // 📄 Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
