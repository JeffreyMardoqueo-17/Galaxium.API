using System;
using System.Collections.Generic;

namespace Galaxium.API.Entities
{
    public class Sale
    {
        public int Id { get; set; }

        // FK
        public int? CustomerId { get; set; }
        public int SellerUserId { get; set; }

        public DateTime SaleDate { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = null!;

        // 🔹 Navegaciones
        public Customer? Customer { get; set; }
        public User SellerUser { get; set; } = null!;

        public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();
    }
}
