namespace Galaxium.API.DTOs;

public record SaleHistorySummaryDto(
    int TotalSales,
    int TotalProductsSold,
    decimal TotalSubTotal,
    decimal TotalDiscount,
    decimal TotalRevenue,
    decimal TotalAmountPaid,
    decimal TotalChangeDelivered,
    decimal AverageTicket
);
