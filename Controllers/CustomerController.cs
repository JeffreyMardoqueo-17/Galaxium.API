using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Galaxium.API.DTOs.Customer;
using Galaxium.API.Entities;
using Galaxium.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public CustomerController(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var customersDto = _mapper.Map<IEnumerable<CustomerResponseDTO>>(customers);
            return Ok(customersDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            try
            {
                var customer = await _customerService.GetByIdCustomerAsync(id);
                if (customer == null)
                    return NotFound();

                var customerDto = _mapper.Map<CustomerResponseDTO>(customer);
                return Ok(customerDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateRequestDTO customerCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var customerEntity = _mapper.Map<Customer>(customerCreateDto);
                var createdCustomer = await _customerService.CreateCustomerAsync(customerEntity);
                var createdCustomerDto = _mapper.Map<CustomerResponseDTO>(createdCustomer);

                return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomerDto.Id }, createdCustomerDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
