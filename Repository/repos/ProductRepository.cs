using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Data;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Repository.repos
{
    public class ProductRepository : IProductRepository
    {
        private readonly GalaxiumDbContext _context;
        public ProductRepository(GalaxiumDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Product
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.CreatedByUser)
                    .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Product
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.CreatedByUser)
                    .FirstOrDefaultAsync(p => p.Id == productId);
        }
        public async Task<Product?> AddProductAsync(Product newProduct)
        {
            _context.Product.Add(newProduct);
            await _context.SaveChangesAsync();
            return newProduct;
        }
        public async Task<int> GetLastSkuNumberByCategoryAsync(int categoryId)
        {
            var lastSku = await _context.Product
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.Id)
                .Select(p => p.SKU)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(lastSku))
                return 0;

            // Ej: HIG-005 → 5
            var parts = lastSku.Split('-');

            if (parts.Length < 2 || !int.TryParse(parts[^1], out var number))
                return 0;

            return number;
        }

    }
}