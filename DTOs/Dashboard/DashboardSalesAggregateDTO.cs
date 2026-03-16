using System;

namespace Galaxium.Api.DTOs.Dashboard
{
    public class DashboardSalesAggregateDTO
    {
        public DateTime PeriodStart { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalTransactions { get; set; }
    }
}
