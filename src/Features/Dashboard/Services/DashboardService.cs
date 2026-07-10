using System;
using System.Collections.Generic;
using System.Globalization;
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

            var todaySales =
                await _dashboardRepository
                    .GetTodaySalesAsync();

            var todayRevenue =
                await _dashboardRepository
                    .GetTodayRevenueAsync();

            var exhaustedProducts =
                await _dashboardRepository
                    .GetExhaustedProductsAsync();

            var recentSales =
                await _dashboardRepository
                    .GetRecentSalesAsync(8);

            // ===== Construcción DTO =====

            return new DashboardSummaryDTO
            {
                TotalCustomers = totalCustomers,

                TotalSales = totalSales,

                TotalRevenue = totalRevenue,

                TotalInvestment = totalInvestment,

                TotalStock = totalStock,

                // 🔥 Profit gerencial
                NetProfit = totalRevenue - totalInvestment,

                TodaySales = todaySales,
                TodayRevenue = todayRevenue,
                ExhaustedProducts = exhaustedProducts,
                RecentSales = recentSales.ToList()
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

        public async Task<DashboardSalesAnalyticsDTO>
            GetSalesAnalyticsAsync(int days, int months, int years)
        {
            if (days <= 0 || days > 60)
                throw new ArgumentException("Days must be between 1 and 60.");

            if (months <= 0 || months > 24)
                throw new ArgumentException("Months must be between 1 and 24.");

            if (years <= 0 || years > 10)
                throw new ArgumentException("Years must be between 1 and 10.");

            var today = DateTime.Today;
            var todayStart = today;
            var tomorrowStart = today.AddDays(1);

            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var yearStart = new DateTime(today.Year, 1, 1);
            var yearEnd = yearStart.AddYears(1);

            var dailyStart = today.AddDays(-(days - 1));
            var monthlyStart = monthStart.AddMonths(-(months - 1));
            var yearlyStart = today.Year - (years - 1);

            var todayRevenue = await _dashboardRepository
                .GetRevenueBetweenAsync(todayStart, tomorrowStart);

            var monthRevenue = await _dashboardRepository
                .GetRevenueBetweenAsync(monthStart, monthEnd);

            var yearRevenue = await _dashboardRepository
                .GetRevenueBetweenAsync(yearStart, yearEnd);

            var dailySeriesRaw = await _dashboardRepository
                .GetDailySalesSeriesAsync(dailyStart);

            var monthlySeriesRaw = await _dashboardRepository
                .GetMonthlySalesSeriesAsync(monthlyStart);

            var yearlySeriesRaw = await _dashboardRepository
                .GetYearlySalesSeriesAsync(yearlyStart);

            var weekdaySeriesRaw = await _dashboardRepository
                .GetWeekdaySalesSeriesAsync();

            var dailySeries = BuildDailySeries(
                dailyStart,
                days,
                dailySeriesRaw
            );

            var monthlySeries = BuildMonthlySeries(
                monthlyStart,
                months,
                monthlySeriesRaw
            );

            var yearlySeries = BuildYearlySeries(
                yearlyStart,
                years,
                yearlySeriesRaw
            );

            var weekdaySeries = weekdaySeriesRaw.ToList();
            var bestWeekday = weekdaySeries
                .OrderByDescending(x => x.TotalAmount)
                .ThenByDescending(x => x.TotalTransactions)
                .FirstOrDefault();

            return new DashboardSalesAnalyticsDTO
            {
                TodayRevenue = todayRevenue,
                CurrentMonthRevenue = monthRevenue,
                CurrentYearRevenue = yearRevenue,
                BestSalesWeekday = bestWeekday is null
                    ? "Sin datos"
                    : GetWeekdayName(bestWeekday.PeriodStart.DayOfWeek),
                BestSalesWeekdayRevenue = bestWeekday?.TotalAmount ?? 0,
                BestSalesWeekdayTransactions = bestWeekday?.TotalTransactions ?? 0,
                DailySeries = dailySeries,
                MonthlySeries = monthlySeries,
                YearlySeries = yearlySeries,
            };
        }

        private static IReadOnlyList<DashboardSalesPointDTO>
            BuildDailySeries(
                DateTime startDate,
                int days,
                IEnumerable<DashboardSalesAggregateDTO> series
            )
        {
            var culture = GetSafeCulture();
            var map = series
                .GroupBy(x => x.PeriodStart.Date)
                .ToDictionary(
                    g => g.Key,
                    g => new DashboardSalesAggregateDTO
                    {
                        PeriodStart = g.Key,
                        TotalAmount = g.Sum(x => x.TotalAmount),
                        TotalTransactions = g.Sum(x => x.TotalTransactions)
                    }
                );
            var points = new List<DashboardSalesPointDTO>(days);

            for (var i = 0; i < days; i += 1)
            {
                var date = startDate.AddDays(i).Date;
                map.TryGetValue(date, out var bucket);

                points.Add(new DashboardSalesPointDTO
                {
                    Label = date.ToString("dd MMM", culture),
                    TotalAmount = bucket?.TotalAmount ?? 0,
                    TotalTransactions = bucket?.TotalTransactions ?? 0
                });
            }

            return points;
        }

        private static IReadOnlyList<DashboardSalesPointDTO>
            BuildMonthlySeries(
                DateTime startMonth,
                int months,
                IEnumerable<DashboardSalesAggregateDTO> series
            )
        {
            var culture = GetSafeCulture();
            var map = series
                .GroupBy(x => (x.PeriodStart.Year, x.PeriodStart.Month))
                .ToDictionary(
                    g => g.Key,
                    g => new DashboardSalesAggregateDTO
                    {
                        PeriodStart = new DateTime(g.Key.Year, g.Key.Month, 1),
                        TotalAmount = g.Sum(x => x.TotalAmount),
                        TotalTransactions = g.Sum(x => x.TotalTransactions)
                    }
                );
            var points = new List<DashboardSalesPointDTO>(months);

            for (var i = 0; i < months; i += 1)
            {
                var date = startMonth.AddMonths(i);
                map.TryGetValue((date.Year, date.Month), out var bucket);

                points.Add(new DashboardSalesPointDTO
                {
                    Label = date.ToString("MMM yyyy", culture),
                    TotalAmount = bucket?.TotalAmount ?? 0,
                    TotalTransactions = bucket?.TotalTransactions ?? 0
                });
            }

            return points;
        }

        private static IReadOnlyList<DashboardSalesPointDTO>
            BuildYearlySeries(
                int startYear,
                int years,
                IEnumerable<DashboardSalesAggregateDTO> series
            )
        {
            var map = series
                .GroupBy(x => x.PeriodStart.Year)
                .ToDictionary(
                    g => g.Key,
                    g => new DashboardSalesAggregateDTO
                    {
                        PeriodStart = new DateTime(g.Key, 1, 1),
                        TotalAmount = g.Sum(x => x.TotalAmount),
                        TotalTransactions = g.Sum(x => x.TotalTransactions)
                    }
                );
            var points = new List<DashboardSalesPointDTO>(years);

            for (var i = 0; i < years; i += 1)
            {
                var year = startYear + i;
                map.TryGetValue(year, out var bucket);

                points.Add(new DashboardSalesPointDTO
                {
                    Label = year.ToString(),
                    TotalAmount = bucket?.TotalAmount ?? 0,
                    TotalTransactions = bucket?.TotalTransactions ?? 0
                });
            }

            return points;
        }

        private static string GetWeekdayName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miercoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sabado",
                DayOfWeek.Sunday => "Domingo",
                _ => "Sin datos"
            };
        }

        private static CultureInfo GetSafeCulture()
        {
            try
            {
                return CultureInfo.GetCultureInfo("es-MX");
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.InvariantCulture;
            }
        }
    }
}
