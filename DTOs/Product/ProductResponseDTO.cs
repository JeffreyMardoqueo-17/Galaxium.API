using System;

namespace Galaxium.API.DTOs.Product
{
   public record ProductResponseDTO(
    int Id,
    string Name,
    string SKU,
    decimal? CostPrice,
    decimal? SalePrice,
    int Stock,
    int MinimumStock,
    bool IsActive,
    DateTime CreatedAt,

    int CategoryId,
    string CategoryName,

    int CreatedByUserId,
    string CreatedByUserName
);

}
