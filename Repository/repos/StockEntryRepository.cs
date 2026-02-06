using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Data;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Repository.repos
{
    public class StockEntryRepository : IStockEntryRepository
    {
        private readonly GalaxiumDbContext _context;
        private readonly IProductRepository _productRepository;
        public StockEntryRepository(GalaxiumDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }
        public async Task<IEnumerable<StockEntry>> GetStockEntriesAsync()
        {
            return await _context.StockEntry
                .AsNoTracking()
                .Include(se => se.Product)
                .Include(se => se.User)
                .ToListAsync();
        }

        public async Task<StockEntry?> GetByIdStockEntryAsync(int id)
        {
            return await _context.StockEntry
                .AsNoTracking()
                .Include(se => se.Product)
                .Include(se => se.User)
                .FirstOrDefaultAsync(se => se.Id == id);
        }

        public async Task<StockEntry> CreateStockEntryAsync(StockEntry stockEntry, Product product)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.StockEntry.Add(stockEntry);
                _context.Product.Update(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 🔥 Cargar relaciones para evitar NullReferenceException
                await _context.Entry(stockEntry).Reference(e => e.Product).LoadAsync();
                await _context.Entry(stockEntry).Reference(e => e.User).LoadAsync();

                return stockEntry;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<StockEntry?> GetLastEntryByProductIdAsync(int productId)
{
    return await _context.StockEntry
        .Where(se => se.ProductId == productId)
        .OrderByDescending(se => se.CreatedAt) //  para saber la fecha en la que entro 
        .FirstOrDefaultAsync();
}



    }
}