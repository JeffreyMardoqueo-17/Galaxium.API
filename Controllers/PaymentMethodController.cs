using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Galaxium.Api.DTOs.PaymentMethod;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public PaymentMethodController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        // GET: api/PaymentMethod
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMethodResponseDto>>> GetAll()
        {
            var paymentMethods = await _paymentMethodService.GetAllAsync();

            return Ok(paymentMethods);
        }

        // GET: api/PaymentMethod/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodResponseDto>> GetById(int id)
        {
            var paymentMethod = await _paymentMethodService.GetByIdAsync(id);
            if (paymentMethod == null)
                return NotFound($"No se encontró el método de pago con ID {id}.");

            return Ok(paymentMethod);
        }
    }
}
