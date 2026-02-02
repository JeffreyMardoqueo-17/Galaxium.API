using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.DTOs.Product
{
   public record ProductFilterRequestDTO(
        int? CategoryId,
        string? Name,

        decimal? MinPrice,
        decimal? MaxPrice,

        int? MinStock,
        int? MaxStock,

        bool? IsActive,

        string? OrderBy,
        bool OrderDescending = true,

        int Page = 1,
        int PageSize = 20
    );
}