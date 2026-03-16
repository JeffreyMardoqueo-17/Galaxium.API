using Galaxium.Api.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Repository.repos;

public class SupplierRepository : ISupplierRepository
{
    private readonly GalaxiumDbContext _context;

    public SupplierRepository(GalaxiumDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _context.Supplier
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int supplierId)
    {
        return await _context.Supplier
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == supplierId);
    }

    public async Task<Supplier> AddAsync(Supplier supplier)
    {
        _context.Supplier.Add(supplier);
        await _context.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier?> UpdateAsync(Supplier supplier)
    {
        var existing = await _context.Supplier.FirstOrDefaultAsync(s => s.Id == supplier.Id);
        if (existing == null)
        {
            return null;
        }

        existing.Name = supplier.Name;
        existing.Phone = supplier.Phone;
        existing.Email = supplier.Email;
        existing.Address = supplier.Address;
        existing.IsActive = supplier.IsActive;

        await _context.SaveChangesAsync();
        return existing;
    }
}
