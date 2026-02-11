using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.DTOs.Dashboard
{
   public class TopSellingProductsResponseDTO
    {
        public int RequestedTop { get; set; }

        public IEnumerable<TopSellingProductDTO> Products { get; set; }
    }
}