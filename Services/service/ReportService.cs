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
        var query = _context.Sale
            .AsNoTracking()
            .Where(s => s.Status == "COMPLETED");

        if (startDate.HasValue)
            query = query.Where(s => s.SaleDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.SaleDate <= endDate.Value);

        return await query
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new SalesByDayItemDto(g.Key, g.Count(), g.Sum(x => x.Total)))
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SalesByProductItemDto>> GetSalesByProductAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.SaleDetail
            .AsNoTracking()
            .Where(d => d.Sale.Status == "COMPLETED");

        if (startDate.HasValue)
            query = query.Where(d => d.Sale.SaleDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(d => d.Sale.SaleDate <= endDate.Value);

        return await query
            .GroupBy(d => new { d.ProductId, d.Product.Name })
            .Select(g => new SalesByProductItemDto(
                g.Key.ProductId,
                g.Key.Name,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Quantity * x.UnitPrice)))
            .OrderByDescending(x => x.QuantitySold)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SalesByCategoryItemDto>> GetSalesByCategoryAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.SaleDetail
            .AsNoTracking()
            .Where(d => d.Sale.Status == "COMPLETED");

        if (startDate.HasValue)
            query = query.Where(d => d.Sale.SaleDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(d => d.Sale.SaleDate <= endDate.Value);

        return await query
            .GroupBy(d => new { d.Product.CategoryId, d.Product.Category.Name })
            .Select(g => new SalesByCategoryItemDto(
                g.Key.CategoryId,
                g.Key.Name,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Quantity * x.UnitPrice)))
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync();
    }

    public async Task<ProfitSummaryDto> GetProfitSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        var salesQuery = _context.Sale
            .AsNoTracking()
            .Where(s => s.Status == "COMPLETED");

        if (startDate.HasValue)
            salesQuery = salesQuery.Where(s => s.SaleDate >= startDate.Value);
        if (endDate.HasValue)
            salesQuery = salesQuery.Where(s => s.SaleDate <= endDate.Value);

        var revenue = await salesQuery.SumAsync(s => (decimal?)s.Total) ?? 0m;

        var purchaseQuery = _context.StockEntry
            .AsNoTracking()
            .Where(s => s.ReferenceType == Enums.StockReferenceType.Purchase);

        if (startDate.HasValue)
            purchaseQuery = purchaseQuery.Where(s => s.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            purchaseQuery = purchaseQuery.Where(s => s.CreatedAt <= endDate.Value);

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
        var query = _context.Purchase
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Details)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(p => p.PurchaseDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(p => p.PurchaseDate <= endDate.Value);

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
}
