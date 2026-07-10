using Galaxium.Api.DTOs.Reports;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Services.service;

public class ReportService : IReportService
{
    private readonly GalaxiumDbContext _context;

    public ReportService(GalaxiumDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SalesByDayItemDto>> GetSalesByDayAsync(DateTime? startDate, DateTime? endDate)
    {
        var (rangeStart, rangeEndExclusive) = NormalizeRange(startDate, endDate);

        var query = _context.Sale
            .AsNoTracking()
            .Where(s => s.Status == "COMPLETED");

        if (rangeStart.HasValue)
            query = query.Where(s => s.SaleDate >= rangeStart.Value);
        if (rangeEndExclusive.HasValue)
            query = query.Where(s => s.SaleDate < rangeEndExclusive.Value);

        var sales = await query
            .Select(s => new { s.SaleDate, s.Total })
            .ToListAsync();

        return sales
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new SalesByDayItemDto(g.Key, g.Count(), g.Sum(x => x.Total)))
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<IReadOnlyList<SalesByProductItemDto>> GetSalesByProductAsync(DateTime? startDate, DateTime? endDate)
    {
        var (rangeStart, rangeEndExclusive) = NormalizeRange(startDate, endDate);

        var query = _context.SaleDetail
            .AsNoTracking()
            .Where(d => d.Sale.Status == "COMPLETED");

        if (rangeStart.HasValue)
            query = query.Where(d => d.Sale.SaleDate >= rangeStart.Value);
        if (rangeEndExclusive.HasValue)
            query = query.Where(d => d.Sale.SaleDate < rangeEndExclusive.Value);

        var details = await query
            .Select(d => new
            {
                d.ProductId,
                ProductName = d.Product.Name,
                d.Quantity,
                d.UnitPrice
            })
            .ToListAsync();

        return details
            .GroupBy(d => new { d.ProductId, d.ProductName })
            .Select(g => new SalesByProductItemDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Quantity * x.UnitPrice)))
            .OrderByDescending(x => x.QuantitySold)
            .ToList();
    }

    public async Task<IReadOnlyList<SalesByCategoryItemDto>> GetSalesByCategoryAsync(DateTime? startDate, DateTime? endDate)
    {
        var (rangeStart, rangeEndExclusive) = NormalizeRange(startDate, endDate);

        var query = _context.SaleDetail
            .AsNoTracking()
            .Where(d => d.Sale.Status == "COMPLETED");

        if (rangeStart.HasValue)
            query = query.Where(d => d.Sale.SaleDate >= rangeStart.Value);
        if (rangeEndExclusive.HasValue)
            query = query.Where(d => d.Sale.SaleDate < rangeEndExclusive.Value);

        var details = await query
            .Select(d => new
            {
                d.Product.CategoryId,
                CategoryName = d.Product.Category.Name,
                d.Quantity,
                d.UnitPrice
            })
            .ToListAsync();

        return details
            .GroupBy(d => new { d.CategoryId, d.CategoryName })
            .Select(g => new SalesByCategoryItemDto(
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Quantity * x.UnitPrice)))
            .OrderByDescending(x => x.TotalAmount)
            .ToList();
    }

    public async Task<ProfitSummaryDto> GetProfitSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        var (rangeStart, rangeEndExclusive) = NormalizeRange(startDate, endDate);

        var salesQuery = _context.Sale
            .AsNoTracking()
            .Where(s => s.Status == "COMPLETED");

        if (rangeStart.HasValue)
            salesQuery = salesQuery.Where(s => s.SaleDate >= rangeStart.Value);
        if (rangeEndExclusive.HasValue)
            salesQuery = salesQuery.Where(s => s.SaleDate < rangeEndExclusive.Value);

        var revenue = await salesQuery.SumAsync(s => (decimal?)s.Total) ?? 0m;

        var purchaseQuery = _context.StockEntry
            .AsNoTracking()
            .Where(s => s.ReferenceType == Enums.StockReferenceType.Purchase);

        if (rangeStart.HasValue)
            purchaseQuery = purchaseQuery.Where(s => s.CreatedAt >= rangeStart.Value);
        if (rangeEndExclusive.HasValue)
            purchaseQuery = purchaseQuery.Where(s => s.CreatedAt < rangeEndExclusive.Value);

        var investment = await purchaseQuery.SumAsync(s => (decimal?)s.TotalCost) ?? 0m;

        return new ProfitSummaryDto(revenue, investment, revenue - investment);
    }

    public async Task<IReadOnlyList<InventorySnapshotItemDto>> GetInventorySnapshotAsync()
    {
        return await _context.Product
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new InventorySnapshotItemDto(
                p.Id,
                p.Name,
                p.SKU,
                p.Stock,
                p.MinimumStock,
                p.Stock <= p.MinimumStock,
                p.Stock == 0))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(DateTime? startDate, DateTime? endDate)
    {
        var (rangeStart, rangeEndExclusive) = NormalizeRange(startDate, endDate);

        var query = _context.Purchase
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Details)
            .AsQueryable();

        if (rangeStart.HasValue)
            query = query.Where(p => p.PurchaseDate >= rangeStart.Value);
        if (rangeEndExclusive.HasValue)
            query = query.Where(p => p.PurchaseDate < rangeEndExclusive.Value);

        return await query
            .OrderByDescending(p => p.PurchaseDate)
            .Select(p => new PurchaseHistoryItemDto(
                p.Id,
                p.PurchaseDate,
                p.SupplierId,
                p.Supplier.Name,
                p.Total,
                p.Details.Count))
            .ToListAsync();
    }

    private static (DateTime? Start, DateTime? EndExclusive) NormalizeRange(DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue && !endDate.HasValue)
        {
            return (null, null);
        }

        var start = (startDate ?? endDate)?.Date;
        var endExclusive = (endDate ?? startDate)?.Date.AddDays(1);

        if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
        {
            throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha final.");
        }

        return (start, endExclusive);
    }
}
