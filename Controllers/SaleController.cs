using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.API.DTOs;
using Galaxium.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaleController : ControllerBase
    {
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public SaleController(ISaleService saleService, IMapper mapper)
        {
            _saleService = saleService;
            _mapper = mapper;
        }

        // ======================================================
        // POST: api/Sale
        // Crear una nueva venta con detalles
        // Ejemplo de JSON para probar (efectivo):
        /*
        {
            "customerId": 1,
            "paymentMethodId": 1,
            "discount": 5,
            "amountPaid": 100,
            "details": [
                { "productId": 1, "quantity": 2 },
                { "productId": 3, "quantity": 1 }
            ]
        }
        */
        // Ejemplo de JSON para probar (tarjeta u otro):
        /*
        {
            "customerId": 1,
            "paymentMethodId": 2,
            "discount": 0,
            "amountPaid": null,
            "details": [
                { "productId": 1, "quantity": 2 }
            ]
        }
        */
        // ======================================================

        [HttpPost]
        public async Task<ActionResult<SaleResponseDto>> CreateSale([FromBody] SaleCreateDto saleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Mapear DTO → Entity
            var sale = _mapper.Map<Sale>(saleDto);

            // 
            // Obtener UserId desde JWT
            // 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

            if (!int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Id de usuario inválido.");

            sale.UserId = userId;

            // Mapear detalles
            var saleDetails = _mapper.Map<IEnumerable<SaleDetail>>(saleDto.Details);

            // Crear la venta completa
            var createdSale = await _saleService.CreateSaleAsync(sale, saleDetails);

            // Mapear Entity---→ Response DTO
            var response = _mapper.Map<SaleResponseDto>(createdSale);

            return CreatedAtAction(nameof(GetSaleById), new { saleId = response.Id }, response);
        }

        // ==========================
        // GET: api/Sale/{saleId}
        // Obtener venta por Id
        // Ejemplo de uso: GET /api/Sale/5
        // ==========================
        [HttpGet("{saleId:int}")]
        public async Task<ActionResult<SaleResponseDto>> GetSaleById(int saleId)
        {
            var sale = await _saleService.GetSaleByIdAsync(saleId);
            var response = _mapper.Map<SaleResponseDto>(sale);
            return Ok(response);
        }

        // ======================================================
        // GET: api/Sale
        // Obtener todas las ventas
        // Ejemplo de uso: GET /api/Sale
        // ======================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleResponseDto>>> GetAllSales()
        {
            var sales = await _saleService.GetAllSalesAsync();
            var response = _mapper.Map<IEnumerable<SaleResponseDto>>(sales);
            return Ok(response);
        }

        // ======================================================
        // GET: api/Sale/ByDateRange?start=2026-02-01&end=2026-02-05
        // Obtener ventas en un rango de fechas
        // Ejemplo de uso: GET /api/Sale/ByDateRange?start=2026-02-01&end=2026-02-05
        // ======================================================
        [HttpGet("ByDateRange")]
        public async Task<ActionResult<IEnumerable<SaleResponseDto>>> GetSalesByDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var sales = await _saleService.GetSalesByDateRangeAsync(start, end);
            var response = _mapper.Map<IEnumerable<SaleResponseDto>>(sales);
            return Ok(response);
        }

        // ======================================================
        // GET: api/Sale/ByCustomer/{customerId}
        // Obtener ventas por cliente
        // Ejemplo de uso: GET /api/Sale/ByCustomer/3
        // ======================================================
        [HttpGet("ByCustomer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<SaleResponseDto>>> GetSalesByCustomer(int customerId)
        {
            var sales = await _saleService.GetSalesByCustomerAsync(customerId);
            var response = _mapper.Map<IEnumerable<SaleResponseDto>>(sales);
            return Ok(response);
        }
    }
}
