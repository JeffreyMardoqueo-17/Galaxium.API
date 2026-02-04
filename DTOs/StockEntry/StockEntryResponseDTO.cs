using System;

namespace Galaxium.Api.DTOs.StockEntry
{
   public record StockEntryResponseDTO(
    int Id,
    int ProductId,
    string ProductName,
    int UserId,
    string UserName,
    int Quantity,
    decimal UnitCost,
    decimal TotalCost,
    bool IsActive,
    DateTime CreatedAt
);

}
