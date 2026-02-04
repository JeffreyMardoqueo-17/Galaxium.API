using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.Api.Repository.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetByIdCustomerAsync(int id);
        Task<Customer> CreateCustomerAsync(Customer customer);

        ///esto es para validad
        Task<bool> ExistsByEmailAsync(string email);
    }
}