using System;

namespace Galaxium.API.DTOs
{
    public record SaleDetailResponseDto(
        int Id,
        int SaleId,
        int ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal UnitCost,
        decimal SubTotal,
        DateTime CreatedAt,
        string ProductName
    );
}
