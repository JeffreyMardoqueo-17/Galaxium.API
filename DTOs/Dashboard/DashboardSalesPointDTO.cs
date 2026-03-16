namespace Galaxium.Api.DTOs.Dashboard
{
    public class DashboardSalesPointDTO
    {
        public string Label { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public int TotalTransactions { get; set; }
    }
}
