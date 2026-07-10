using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Repository.Interfaces;
using Galaxium.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Repository.repos
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly GalaxiumDbContext _context;
        public PaymentMethodRepository(GalaxiumDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Entities.PaymentMethod>> GetAllAsync()
        {
            return await _context.PaymentMethod
                .AsNoTracking()
                .Where(pm => pm.IsActive)
                .ToListAsync();
        }
        public async Task<Entities.PaymentMethod?> GetByIdAsync(int id)
        {
            return await _context.PaymentMethod.
                AsNoTracking()
                .FirstOrDefaultAsync(pm => pm.Id == id && pm.IsActive);

        }
    }
}