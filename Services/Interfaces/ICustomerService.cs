using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.Interfaces
{
    public interface ICustomerService
    {
        
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetByIdCustomerAsync(int id);
        Task<Customer> CreateCustomerAsync(Customer customer);

    }
}