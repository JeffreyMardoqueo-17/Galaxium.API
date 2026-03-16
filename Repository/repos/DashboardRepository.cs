using System.Collections.Generic;
using System;
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

        public async Task<int> GetTodaySalesAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _context.Sale
                .Where(s => s.Status == "COMPLETED" && s.SaleDate >= today && s.SaleDate < tomorrow)
                .CountAsync();
        }

        public async Task<decimal> GetTodayRevenueAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _context.Sale
                .Where(s => s.Status == "COMPLETED" && s.SaleDate >= today && s.SaleDate < tomorrow)
                .SumAsync(s => (decimal?)s.Total) ?? 0m;
        }

        public async Task<int> GetExhaustedProductsAsync()
        {
            return await _context.Product
                .Where(p => p.Stock <= 0)
                .CountAsync();
        }

        public async Task<IEnumerable<DashboardRecentSaleDto>> GetRecentSalesAsync(int top)
        {
            return await _context.Sale
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.Status == "COMPLETED")
                .OrderByDescending(s => s.SaleDate)
                .Take(top)
                .Select(s => new DashboardRecentSaleDto
                {
                    SaleId = s.Id,
                    InvoiceNumber = s.InvoiceNumber ?? string.Empty,
                    SaleDate = s.SaleDate,
                    Total = s.Total,
                    SellerName = s.User.FullName
                })
                .ToListAsync();
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
                    TotalRevenue =
                        g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(top)
                .ToListAsync();
        }

        public async Task<decimal> GetRevenueBetweenAsync(
            DateTime startDate,
            DateTime endDateExclusive
        )
        {
            return await _context.Sale
                .Where(
                    s => s.Status == "COMPLETED"
                      && s.SaleDate >= startDate
                      && s.SaleDate < endDateExclusive
                )
                .SumAsync(s => (decimal?)s.Total) ?? 0;
        }

        public async Task<IEnumerable<DashboardSalesAggregateDTO>>
            GetDailySalesSeriesAsync(DateTime startDate)
        {
            return await _context.Sale
                .Where(
                    s => s.Status == "COMPLETED"
                      && s.SaleDate >= startDate
                )
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new DashboardSalesAggregateDTO
                {
                    PeriodStart = g.Key,
                    TotalAmount = g.Sum(x => x.Total),
                    TotalTransactions = g.Count()
                })
                .OrderBy(x => x.PeriodStart)
                .ToListAsync();
        }

        public async Task<IEnumerable<DashboardSalesAggregateDTO>>
            GetMonthlySalesSeriesAsync(DateTime startDate)
        {
            var grouped = await _context.Sale
                .Where(
                    s => s.Status == "COMPLETED"
                      && s.SaleDate >= startDate
                )
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalAmount = g.Sum(x => x.Total),
                    TotalTransactions = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return grouped.Select(g => new DashboardSalesAggregateDTO
            {
                PeriodStart = new DateTime(g.Year, g.Month, 1),
                TotalAmount = g.TotalAmount,
                TotalTransactions = g.TotalTransactions
            });
        }

        public async Task<IEnumerable<DashboardSalesAggregateDTO>>
            GetYearlySalesSeriesAsync(int startYear)
        {
            var grouped = await _context.Sale
                .Where(
                    s => s.Status == "COMPLETED"
                      && s.SaleDate.Year >= startYear
                )
                .GroupBy(s => s.SaleDate.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalAmount = g.Sum(x => x.Total),
                    TotalTransactions = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            return grouped.Select(g => new DashboardSalesAggregateDTO
            {
                PeriodStart = new DateTime(g.Year, 1, 1),
                TotalAmount = g.TotalAmount,
                TotalTransactions = g.TotalTransactions
            });
        }

        public async Task<IEnumerable<DashboardSalesAggregateDTO>>
            GetWeekdaySalesSeriesAsync()
        {
            var sales = await _context.Sale
                .Where(s => s.Status == "COMPLETED")
                .Select(s => new { s.SaleDate, s.Total })
                .ToListAsync();

            return sales
                .GroupBy(s => (int)s.SaleDate.DayOfWeek)
                .Select(g => new DashboardSalesAggregateDTO
                {
                    PeriodStart = new DateTime(2000, 1, 2 + g.Key),
                    TotalAmount = g.Sum(x => x.Total),
                    TotalTransactions = g.Count()
                })
                .ToList();
        }
    }
}
