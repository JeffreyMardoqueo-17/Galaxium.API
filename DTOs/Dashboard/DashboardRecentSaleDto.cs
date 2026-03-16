namespace Galaxium.Api.DTOs.Dashboard;

public class DashboardRecentSaleDto
{
    public int SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal Total { get; set; }
    public string SellerName { get; set; } = string.Empty;
}
