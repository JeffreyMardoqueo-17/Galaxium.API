using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Galaxium.Api.DTOs.Dashboard
{
    public class TopSellingProductDTO
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int TotalSold { get; set; }

        public decimal RevenueGenerated { get; set; }
    }
}
