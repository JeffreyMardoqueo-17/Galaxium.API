using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.DTOs.productphoto;

namespace Galaxium.Api.DTOs.Product
{
   public record ProductWithPhotosResponseDTO(
        int Id,
        string Name,
        string SKU,
        string? Barcode,
        decimal? CostPrice,
        decimal? SalePrice,
        int Stock,
        int MinimumStock,
        bool IsActive,
        DateTime CreatedAt,
        int CategoryId,
        string CategoryName,
        int CreatedByUserId,
        string CreatedByUserName,
        IEnumerable<ProductPhotoResponseDTO> Photos
    );

}