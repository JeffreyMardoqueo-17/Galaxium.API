using AutoMapper;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.DTOs.Product;
using Galaxium.API.Entities;
using Galaxium.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductController(
            IProductService productService,
            IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        // ===============================
        // GET: api/product
        // ===============================
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetProducts()
        {
            var products = await _productService.GetProductsAsync();
            var response = _mapper.Map<IEnumerable<ProductResponseDTO>>(products);

            return Ok(response);
        }

        // ===============================
        // GET: api/product/{id}
        // ===============================
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<ProductResponseDTO>> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            var response = _mapper.Map<ProductResponseDTO>(product);

            return Ok(response);
        }

        // ===============================
        // POST: api/product
        // ===============================
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductResponseDTO>> CreateProduct(
    [FromBody] ProductCreateRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var productEntity = _mapper.Map<Product>(request);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            if (!int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user id.");

            productEntity.CreatedByUserId = userId;

            var createdProduct = await _productService.AddProductAsync(productEntity);
            var response = _mapper.Map<ProductResponseDTO>(createdProduct);

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = response.Id },
                response
            );
        }

        // ===============================
        // GET: api/product/filter
        // GET /api/product/filter?
        // categoryId=2
        // &minPrice=50
        // &maxPrice=200
        // &minStock=10
        // &maxStock=20
        // &orderBy=SalePrice
        // &orderDescending=false
        // &page=1
        // &pageSize=20

        // ===============================
        [HttpGet("filter")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetProductsByFilter(
            [FromQuery] ProductFilterRequestDTO filterDto)
        {
            // DTO (API) → Model (Service/Repository)
            var filterModel = _mapper.Map<ProductFilterModel>(filterDto);

            var products = await _productService.GetProductsFilterAsync(filterModel);

            var response = _mapper.Map<IEnumerable<ProductResponseDTO>>(products);

            return Ok(response);
        }


    }
}
