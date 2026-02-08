using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Galaxium.API.DTOs;
using Galaxium.API.Entities;
using Galaxium.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaleDetailController : ControllerBase
    {
        private readonly ISaleDetailService _saleDetailService;
        private readonly IMapper _mapper;

        public SaleDetailController(ISaleDetailService saleDetailService, IMapper mapper)
        {
            _saleDetailService = saleDetailService;
            _mapper = mapper;
        }

        // ======================================================
        // GET: api/SaleDetail/BySale/{saleId}
        // Obtener todos los detalles de una venta por Id
        // Ejemplo de uso: GET /api/SaleDetail/BySale/5
        // ======================================================
        [HttpGet("BySale/{saleId:int}")]
        public async Task<ActionResult<IEnumerable<SaleDetailResponseDto>>> GetDetailsBySaleId(int saleId)
        {
            var details = await _saleDetailService.GetDetailsBySaleIdAsync(saleId);
            var response = _mapper.Map<IEnumerable<SaleDetailResponseDto>>(details);
            return Ok(response);
        }

        // ======================================================
        // GET: api/SaleDetail/ByProduct/{productId}
        // Obtener todos los detalles donde se vendió un producto específico
        // Ejemplo de uso: GET /api/SaleDetail/ByProduct/3
        // ======================================================
        [HttpGet("ByProduct/{productId:int}")]
        public async Task<ActionResult<IEnumerable<SaleDetailResponseDto>>> GetDetailsByProductId(int productId)
        {
            var details = await _saleDetailService.GetDetailsByProductIdAsync(productId);
            var response = _mapper.Map<IEnumerable<SaleDetailResponseDto>>(details);
            return Ok(response);
        }
    }
}
