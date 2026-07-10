using System;

namespace Galaxium.API.Entities
{
    // Modelo para filtros de productos (no requiere data annotations de base de datos)
    public class ProductFilterModel
    {
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Barcode { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public bool? IsActive { get; set; }
        public string OrderBy { get; set; } = "CreatedAt";
        public bool OrderDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
