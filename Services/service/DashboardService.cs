using System;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.DTOs.Dashboard;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;

namespace Galaxium.Api.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository
                ?? throw new ArgumentNullException(nameof(dashboardRepository));
        }

        // ============================================================
        // SUMMARY
        // ============================================================

        public async Task<DashboardSummaryDTO>
            GetDashboardSummaryAsync()
        {
            // ===== Obtener KPIs =====

            var totalCustomers =
                await _dashboardRepository
                    .GetTotalCustomersAsync();

            var totalSales =
                await _dashboardRepository
                    .GetTotalSalesAsync();

            var totalRevenue =
                await _dashboardRepository
                    .GetTotalRevenueAsync();

            var totalInvestment =
                await _dashboardRepository
                    .GetTotalInvestmentAsync();

            var totalStock =
                await _dashboardRepository
                    .GetTotalStockAsync();

            // ===== Construcción DTO =====

            return new DashboardSummaryDTO
            {
                TotalCustomers = totalCustomers,

                TotalSales = totalSales,

                TotalRevenue = totalRevenue,

                TotalInvestment = totalInvestment,

                TotalStock = totalStock,

                // 🔥 Profit gerencial
                NetProfit = totalRevenue - totalInvestment
            };
        }

        // ============================================================
        // TOP PRODUCTS
        // ============================================================

        public async Task<TopSellingProductsResponseDTO>
            GetTopSellingProductsAsync(int top)
        {
            if (top <= 0)
                throw new ArgumentException(
                    "Top must be greater than zero."
                );

            if (top > 100)
                throw new ArgumentException(
                    "Top cannot exceed 100 records."
                );

            var products =
                await _dashboardRepository
                    .GetTopSellingProductsAsync(top);

            return new TopSellingProductsResponseDTO
            {
                RequestedTop = top,
                Products = products.ToList()
            };
        }
    }
}
