using AutoMapper;
using Galaxium.Api.DTOs.Product;
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

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Token inválido: no contiene UserId");

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("UserId inválido en el token");

            var productEntity = _mapper.Map<Product>(request);

            var createdProduct = await _productService.AddProductAsync(productEntity, userId);

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
        public async Task<ActionResult<IEnumerable<ProductWithPhotosResponseDTO>>> GetProductsByFilter(
            [FromQuery] ProductFilterRequestDTO filterDto)
        {
            // DTO (API) → Model (Service/Repository)
            var filterModel = _mapper.Map<ProductFilterModel>(filterDto);

            var products = await _productService.GetProductsFilterAsync(filterModel);

            var response = _mapper.Map<IEnumerable<ProductWithPhotosResponseDTO>>(products);

            return Ok(response);
        }

        // ===============================
        // PATCH: api/product/price
        // ===============================
        [HttpPatch("price")]
        [Authorize]
        public async Task<ActionResult<ProductResponseDTO>> UpdateProductPrice(
     [FromBody] ProductUpdatePriceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedProduct = await _productService.UpdateProductPriceAsync(dto.ProductId, dto.NewPrice);

                if (updatedProduct == null)
                    return NotFound($"No se encontró el producto con Id {dto.ProductId}");

                // Convertimos a DTO de respuesta
                var response = _mapper.Map<ProductResponseDTO>(updatedProduct);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Captura reglas de negocio, por ejemplo stock insuficiente
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Captura errores de validación, por ejemplo precio negativo
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                // Error inesperado
                return StatusCode(500, new { message = "Ocurrió un error al actualizar el precio" });
            }
        }
        // ===============================
        // GET: api/product/with-photos
        // ===============================
        [HttpGet("with-photos")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ProductWithPhotosResponseDTO>>> GetProductsWithPhotos()
        {
            try
            {
                // 🔹 Service (reglas de negocio)
                var products = await _productService.GetProductsWithPhotosAsync();

                // 🔹 Mapping → DTO
                var response = _mapper.Map<IEnumerable<ProductWithPhotosResponseDTO>>(products);

                return Ok(response);
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("No existen productos"))
                    return NotFound(new { message = ex.Message });

                if (ex.Message.Contains("sin fotos"))
                    return BadRequest(new { message = ex.Message });

                if (ex.Message.Contains("múltiples fotos primarias"))
                    return Conflict(new { message = ex.Message });

                // 🔴 Error inesperado
                return StatusCode(500, new
                {
                    message = "Ocurrió un error al obtener productos con fotos",
                    detail = ex.Message
                });
            }
        }


    }
}
