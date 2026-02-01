using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Data;
using Galaxium.API.Entities;
using Microsoft.EntityFrameworkCore;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.API.Repository.repos
{
    public class ProductPhotoRepository : IProductPhotoRepository
    {
        private readonly GalaxiumDbContext _context;

        public ProductPhotoRepository(GalaxiumDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ProductPhoto photo)
        {
            _context.ProductPhoto.Add(photo);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountByProductAsync(int productId)
        {
            return await _context.ProductPhoto
                .CountAsync(p => p.ProductId == productId);
        }
    }
}