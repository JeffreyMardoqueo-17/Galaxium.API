using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.Entities
{
    public class ProductPhoto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Product Product { get; set; } = null!;

    }
}