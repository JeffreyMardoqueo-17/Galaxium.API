using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Common;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.service
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailService _emailService;
        public CustomerService(ICustomerRepository customerRepository, IEmailService emailService)
        {
            _customerRepository = customerRepository;
            _emailService = emailService;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();
            if (customers == null)
            {
                throw new BusinessException("No customers found.");
            }
            return customers;
        }
        public async Task<Customer?> GetByIdCustomerAsync(int id)
        {
            if (id <= 0)
                throw new BusinessException("Invalid customer ID.");
            var customer = await _customerRepository.GetByIdCustomerAsync(id);
            if (customer == null)
                throw new BusinessException($"Customer with ID {id} not found.");

            return customer;
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.FullName))
                throw new BusinessException("Customer full name is required.");

            customer.CreatedAt = DateTime.UtcNow;

            Customer createdCustomer;
            try
            {
                createdCustomer = await _customerRepository.CreateCustomerAsync(customer);
            }
            catch (Exception ex)
            {
                throw new BusinessException("Error creating customer. " + ex.Message);
            }

            try
            {
                await _emailService.EnviarEmailRegistroCliente(customer.Email, customer.FullName);
            }
            catch (Exception ex)
            {
                throw new BusinessException("Error sending welcome email. " + ex.Message);
            }

            return createdCustomer;
        }

    }
}