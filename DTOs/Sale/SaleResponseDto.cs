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
            decimal AmountPaid,     // Dinero recibido
            decimal ChangeAmount,   // Vuelto entregado
            string Status,
            string? InvoiceNumber,
            DateTime SaleDate,
            DateTime CreatedAt,
            List<SaleDetailResponseDto> Details
        );
    }
