using System;
using System.Collections.Generic;

namespace Galaxium.API.DTOs
{
    public record SaleResponseDto(
        int Id,
        int? CustomerId,
        int UserId,
        int PaymentMethodId,
        decimal SubTotal,
        decimal Discount,
        decimal Total,
        string Status,
        string? InvoiceNumber,
        DateTime SaleDate,
        DateTime CreatedAt,
        List<SaleDetailResponseDto> Details
    );
}
