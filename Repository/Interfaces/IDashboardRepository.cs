using System.Collections.Generic;
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

        Task<IEnumerable<TopSellingProductDTO>>
            GetTopSellingProductsAsync(int top);
    }
}
