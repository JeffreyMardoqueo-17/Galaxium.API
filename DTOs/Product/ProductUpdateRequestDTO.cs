using System;

namespace Galaxium.API.DTOs.Product
{
    public record ProductUpdateRequestDTO(
        int CategoryId,
        string Name,
        string SKU,
        decimal SalePrice,
        int MinimumStock,
        bool IsActive
    );
}
