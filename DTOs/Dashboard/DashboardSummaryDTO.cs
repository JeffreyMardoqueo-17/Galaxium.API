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
    }
}
