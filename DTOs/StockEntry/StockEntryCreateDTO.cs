using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.DTOs.StockEntry
{
    public record StockEntryCreateDTO(
        int ProductId,
        int Quantity,
        decimal UnitCost
    );
}