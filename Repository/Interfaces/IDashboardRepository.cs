using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Galaxium.Api.DTOs.Dashboard;

namespace Galaxium.Api.Repository.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetTotalCustomersAsync();

        Task<int> GetTotalSalesAsync();

        Task<decimal> GetTotalRevenueAsync();

        Task<decimal> GetTotalInvestmentAsync();

        Task<int> GetTotalStockAsync();

        Task<int> GetTodaySalesAsync();

        Task<decimal> GetTodayRevenueAsync();

        Task<int> GetExhaustedProductsAsync();

        Task<IEnumerable<DashboardRecentSaleDto>> GetRecentSalesAsync(int top);

        Task<IEnumerable<TopSellingProductDTO>>
            GetTopSellingProductsAsync(int top);

        Task<decimal> GetRevenueBetweenAsync(
            DateTime startDate,
            DateTime endDateExclusive
        );

        Task<IEnumerable<DashboardSalesAggregateDTO>>
            GetDailySalesSeriesAsync(DateTime startDate);

        Task<IEnumerable<DashboardSalesAggregateDTO>>
            GetMonthlySalesSeriesAsync(DateTime startDate);

        Task<IEnumerable<DashboardSalesAggregateDTO>>
            GetYearlySalesSeriesAsync(int startYear);

        Task<IEnumerable<DashboardSalesAggregateDTO>>
            GetWeekdaySalesSeriesAsync();
    }
}
