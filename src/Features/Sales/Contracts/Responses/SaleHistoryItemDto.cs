namespace Galaxium.API.DTOs;

public record SaleHistoryItemDto(
    int Id,
    string InvoiceNumber,
    DateTime SaleDate,
    string CustomerName,
    string SellerName,
    string PaymentMethod,
    int ProductsSold,
    decimal SubTotal,
    decimal Discount,
    decimal Total,
    decimal AmountPaid,
    decimal ChangeAmount
);
