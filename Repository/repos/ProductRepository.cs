using Galaxium.API.Data;
using Galaxium.API.Entities;
using Galaxium.API.Models;
using Galaxium.API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Repository.repos
{
    public class ProductRepository : IProductRepository, IProductFilterRepository
    {
        private readonly GalaxiumDbContext _context;

        public ProductRepository(GalaxiumDbContext context)
        {
            _context = context;
        }

        // ===============================
        // GET ALL
        // ===============================
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Product
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.CreatedByUser)
                .ToListAsync();
        }

        // ===============================
        // GET BY ID
        // ===============================
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Product
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        // ===============================
        // CREATE
        // ===============================
        public async Task<Product> AddProductAsync(Product newProduct)
        {
            _context.Product.Add(newProduct);
            await _context.SaveChangesAsync();

            // 🔥 CLAVE: recargar con relaciones
            return await _context.Product
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.CreatedByUser)
                .FirstAsync(p => p.Id == newProduct.Id);
        }

        // ===============================
        // SKU SUPPORT
        // ===============================
        public async Task<int> GetLastSkuNumberByCategoryAsync(int categoryId)
        {
            var lastSku = await _context.Product
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.Id)
                .Select(p => p.SKU)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(lastSku))
                return 0;

            var parts = lastSku.Split('-');
            if (parts.Length < 2 || !int.TryParse(parts[^1], out var number))
                return 0;

            return number;
        }

        // ===============================
        // FILTER
        // ===============================
        public async Task<IEnumerable<Product>> GetProductsFilterAsync(ProductFilterModel filter)
        {
            IQueryable<Product> query = _context.Product
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.CreatedByUser);

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(p => p.Name.Contains(filter.Name));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.SalePrice >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.SalePrice <= filter.MaxPrice.Value);

            if (filter.MinStock.HasValue)
                query = query.Where(p => p.Stock >= filter.MinStock.Value);

            if (filter.MaxStock.HasValue)
                query = query.Where(p => p.Stock <= filter.MaxStock.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            // ⚠️ Protección mínima para OrderBy dinámico
            query = filter.OrderDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, filter.OrderBy))
                : query.OrderBy(e => EF.Property<object>(e, filter.OrderBy));

            query = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);

            return await query.ToListAsync();
        }
    }
}
