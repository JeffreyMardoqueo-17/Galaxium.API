using System;

namespace Galaxium.Api.DTOs.Dashboard
{
    public class DashboardSummaryDTO
    {
        public int TotalCustomers { get; set; }

        public int TotalSales { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal TotalInvestment { get; set; }

        public int TotalStock { get; set; }

        public decimal NetProfit { get; set; }

        public int TodaySales { get; set; }

        public decimal TodayRevenue { get; set; }

        public int ExhaustedProducts { get; set; }

        public List<DashboardRecentSaleDto> RecentSales { get; set; } = new();
    }
}
