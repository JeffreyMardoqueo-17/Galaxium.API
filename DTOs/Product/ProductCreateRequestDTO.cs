using System;

namespace Galaxium.API.DTOs.Product
{
    public record ProductCreateRequestDTO(
        int CategoryId,
        string Name,
        string SKU,
        decimal CostPrice,
        decimal SalePrice,
        int InitialStock,
        int MinimumStock,
        bool IsActive
    );
}
