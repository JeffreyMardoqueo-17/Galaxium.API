using System;

namespace Galaxium.API.DTOs.Product
{
    public record ProductUpdateRequestDTO(
    int CategoryId,
    string Name,
        decimal? CostPrice,
        decimal? SalePrice,
    int MinimumStock,
        string? UnitOfMeasure,
   string? Barcode,   // 
    bool IsActive
);
}
