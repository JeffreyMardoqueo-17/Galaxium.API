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
    public class CustomerRepository : ICustomerRepository
    {
        private readonly GalaxiumDbContext _context;
        public CustomerRepository(GalaxiumDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customer
                    .AsNoTracking()
                    .Include(c => c.Sales)
                    .ToListAsync();
        }
        public async Task<Customer?> GetByIdCustomerAsync(int id)
        {
            return await _context.Customer
                    .AsNoTracking()
                    .Include(c => c.Sales)
                    .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            _context.Customer.Add(customer);
            await  _context.SaveChangesAsync();
            return customer;
        }
    }
}