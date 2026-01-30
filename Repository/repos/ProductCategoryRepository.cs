using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Data;
using Galaxium.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Repository.repos
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly GalaxiumDbContext _context;
        public ProductCategoryRepository(GalaxiumDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ProductCategory>> GetAllProductCategories()
        {
            return await _context.ProductCategory
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<ProductCategory?> GetProductCategoryById(int id)
        {
            return await _context.ProductCategory
                .AsNoTracking()
                .FirstOrDefaultAsync(pc => pc.Id == id);
        }
        public async Task<ProductCategory> CreateProductCategory(ProductCategory productCategory)
        {
            _context.ProductCategory.Add(productCategory);
            await  _context.SaveChangesAsync();
            return productCategory;
        }
    }
}