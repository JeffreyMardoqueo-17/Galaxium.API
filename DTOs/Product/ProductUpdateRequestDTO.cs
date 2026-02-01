using System;

namespace Galaxium.API.DTOs.Product
{
    public record ProductUpdateRequestDTO(
        int CategoryId,
        string Name,
        decimal SalePrice,
        int MinimumStock,
        bool IsActive
    );
}
