using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.Entities
{
    public class SaleDetail
    {
        public int Id { get; set; }

        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Columna calculada en SQL
        public decimal SubTotal { get; private set; }

        public Sale Sale { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }

}