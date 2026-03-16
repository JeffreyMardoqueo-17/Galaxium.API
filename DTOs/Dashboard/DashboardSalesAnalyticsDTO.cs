using System.Collections.Generic;

namespace Galaxium.Api.DTOs.Dashboard
{
    public class DashboardSalesAnalyticsDTO
    {
        public decimal TodayRevenue { get; set; }

        public decimal CurrentMonthRevenue { get; set; }

        public decimal CurrentYearRevenue { get; set; }

        public string BestSalesWeekday { get; set; } = string.Empty;

        public decimal BestSalesWeekdayRevenue { get; set; }

        public int BestSalesWeekdayTransactions { get; set; }

        public IEnumerable<DashboardSalesPointDTO> DailySeries { get; set; }
            = new List<DashboardSalesPointDTO>();

        public IEnumerable<DashboardSalesPointDTO> MonthlySeries { get; set; }
            = new List<DashboardSalesPointDTO>();

        public IEnumerable<DashboardSalesPointDTO> YearlySeries { get; set; }
            = new List<DashboardSalesPointDTO>();
    }
}
