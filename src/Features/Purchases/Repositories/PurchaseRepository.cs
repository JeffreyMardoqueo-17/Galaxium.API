using Galaxium.Api.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Repository.repos;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly GalaxiumDbContext _context;

    public PurchaseRepository(GalaxiumDbContext context)
    {
        _context = context;
    }

    public async Task<Purchase> AddAsync(Purchase purchase)
    {
        _context.Purchase.Add(purchase);
        await _context.SaveChangesAsync();

        return await _context.Purchase
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Details)
                .ThenInclude(d => d.Product)
            .FirstAsync(p => p.Id == purchase.Id);
    }

    public async Task<IEnumerable<Purchase>> GetAllAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Purchase
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Details)
                .ThenInclude(d => d.Product)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PurchaseDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PurchaseDate <= endDate.Value);
        }

        return await query
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    public async Task<Purchase?> GetByIdAsync(int purchaseId)
    {
        return await _context.Purchase
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Details)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(p => p.Id == purchaseId);
    }
}
