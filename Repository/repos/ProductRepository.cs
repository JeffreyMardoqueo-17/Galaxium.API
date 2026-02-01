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
    public class ProductRepository: IProductRepository
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
                    .Include (p => p.CreatedByUser)
                    .FirstOrDefaultAsync(p => p.Id == productId);
        }
        public async Task<Product?> AddProductAsync(Product newProduct)
        {
            _context.Product.Add(newProduct);
            await _context.SaveChangesAsync();
            return newProduct;
        }
    }
}