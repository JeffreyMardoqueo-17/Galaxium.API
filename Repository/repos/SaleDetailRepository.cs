using Microsoft.EntityFrameworkCore;
using Galaxium.API.Entities;
using Galaxium.API.Data;
using Galaxium.Api.Repository.Interfaces;

namespace Galaxium.API.Repositories.Implementations
{
    public class SaleDetailRepository : ISaleDetailRepository
    {
        private readonly GalaxiumDbContext _context;

        public SaleDetailRepository(GalaxiumDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<SaleDetail> details)
        {
            await _context.SaleDetail.AddRangeAsync(details);
        }

        public async Task<IEnumerable<SaleDetail>> GetBySaleIdAsync(int saleId)
        {
            return await _context.SaleDetail
                .Where(d => d.SaleId == saleId)
                .Include(d => d.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<SaleDetail>> GetByProductIdAsync(int productId)
        {
            return await _context.SaleDetail
                .Where(d => d.ProductId == productId)
                .Include(d => d.Sale)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
