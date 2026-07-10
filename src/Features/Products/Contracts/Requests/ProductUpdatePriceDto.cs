using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.DTOs.Product
{
    public record ProductUpdatePriceDto(
        int ProductId,
        decimal NewPrice
    );

}