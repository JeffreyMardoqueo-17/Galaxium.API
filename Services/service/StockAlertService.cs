using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Services.service;

public class StockAlertService : IStockAlertService
{
    private readonly GalaxiumDbContext _context;

    public StockAlertService(GalaxiumDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockAlert>> RefreshAlertsAsync()
    {
        var products = await _context.Product
            .AsNoTracking()
            .ToListAsync();

        var now = DateTime.UtcNow;
        var expected = new List<(int ProductId, StockAlertType Type, string Message)>();

        foreach (var product in products)
        {
            if (product.Stock <= 0)
            {
                expected.Add((
                    product.Id,
                    StockAlertType.Exhausted,
                    $"Producto {product.Name} agotado."));
            }
            else if (product.Stock <= product.MinimumStock)
            {
                expected.Add((
                    product.Id,
                    StockAlertType.LowStock,
                    $"Producto {product.Name} en stock bajo ({product.Stock})."));
            }

            var lastMovement = await _context.StockEntry
                .AsNoTracking()
                .Where(s => s.ProductId == product.Id)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => (DateTime?)s.CreatedAt)
                .FirstOrDefaultAsync();

            if (!lastMovement.HasValue || (now - lastMovement.Value).TotalDays >= 30)
            {
                expected.Add((
                    product.Id,
                    StockAlertType.NoMovement,
                    $"Producto {product.Name} sin movimiento en 30 dias."));
            }
        }

        var currentActive = await _context.StockAlert
            .Where(a => a.IsActive)
            .ToListAsync();

        foreach (var current in currentActive)
        {
            var stillRequired = expected.Any(e => e.ProductId == current.ProductId && e.Type == current.AlertType);
            if (!stillRequired)
            {
                current.IsActive = false;
                current.ResolvedAt = now;
            }
        }

        foreach (var item in expected)
        {
            var exists = currentActive.Any(a => a.ProductId == item.ProductId && a.AlertType == item.Type && a.IsActive);
            if (exists)
            {
                continue;
            }

            _context.StockAlert.Add(new StockAlert
            {
                ProductId = item.ProductId,
                AlertType = item.Type,
                Message = item.Message,
                IsActive = true,
                CreatedAt = now
            });
        }

        await _context.SaveChangesAsync();

        return await GetActiveAlertsAsync();
    }

    public async Task<IReadOnlyList<StockAlert>> GetActiveAlertsAsync()
    {
        return await _context.StockAlert
            .AsNoTracking()
            .Include(a => a.Product)
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<StockAlert?> ResolveAlertAsync(int alertId)
    {
        var alert = await _context.StockAlert.FirstOrDefaultAsync(a => a.Id == alertId);
        if (alert == null)
            return null;

        alert.IsActive = false;
        alert.ResolvedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return alert;
    }
}
