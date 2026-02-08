using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Galaxium.API.DTOs
{
    public record SaleCreateDto(
        int? CustomerId,
        int PaymentMethodId,
        decimal Discount,
        List<SaleDetailCreateDto> Details
    );
}
