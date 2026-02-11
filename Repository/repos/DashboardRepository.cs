using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Galaxium.Api.DTOs.Dashboard;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Data;

namespace Galaxium.Api.Repository.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly GalaxiumDbContext _context;

        public DashboardRepository(GalaxiumDbContext context)
        {
            _context = context;
        }

        // =========================
        // KPIs
        // =========================

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _context.Customer.CountAsync();
        }

        public async Task<int> GetTotalSalesAsync()
        {
            return await _context.Sale
                .Where(s => s.Status == "COMPLETED")
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Sale
                .Where(s => s.Status == "COMPLETED")
                .SumAsync(s => (decimal?)s.Total) ?? 0;
        }

        // 🔥 NUEVO — INVERSIÓN TOTAL
        public async Task<decimal> GetTotalInvestmentAsync()
        {
            return await _context.StockEntry
                .Where(se => se.ReferenceType == Enums.StockReferenceType.Purchase)
                .SumAsync(se => (decimal?)se.TotalCost) ?? 0;
        }

        public async Task<int> GetTotalStockAsync()
        {
            return await _context.Product
                .Where(p => p.IsActive)
                .SumAsync(p => (int?)p.Stock) ?? 0;
        }

        // =========================
        // TOP PRODUCTOS
        // =========================

        public async Task<IEnumerable<TopSellingProductDTO>>
            GetTopSellingProductsAsync(int top)
        {
            return await _context.SaleDetail
                .Where(sd => sd.Sale.Status == "COMPLETED")
                .GroupBy(sd => new
                {
                    sd.ProductId,
                    sd.Product.Name
                })
                .Select(g => new TopSellingProductDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(x => x.Quantity),
                    RevenueGenerated =
                        g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(top)
                .ToListAsync();
        }
    }
}
