using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Enums;

namespace Galaxium.Api.DTOs.StockEntry
{
  public record StockEntryCreateDTO(
        int ProductId,
        int Quantity,
        StockReferenceType ReferenceType,
        int? ReferenceId,
        decimal? UnitCost
    );
}