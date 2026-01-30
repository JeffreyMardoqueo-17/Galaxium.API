using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.DTOs.ProductCategory;
using Microsoft.AspNetCore.Mvc;
    using AutoMapper;
using Galaxium.API.Entities;
using Microsoft.AspNetCore.Authorization; //
namespace Galaxium.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryService _productCategoryService;
        private readonly IMapper _mapper;

        public ProductCategoryController(IProductCategoryService productCategoryService, IMapper mapper)
        {
            _productCategoryService = productCategoryService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllProductCategories()
        {
            var categories = await _productCategoryService.GetAllProductCategories();
            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetProductCategoryById(int id)
        {
            var category = await _productCategoryService.GetProductCategoryById(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProductCategory([FromBody] ProductCategoryRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("ProductCategory cannot be null.");

            // Aquí mapear explícitamente dto a entidad
            var entity = _mapper.Map<ProductCategory>(dto);

            var createdCategory = await _productCategoryService.CreateProductCategory(entity);

            return CreatedAtAction(
                nameof(GetProductCategoryById),
                new { id = createdCategory.Id },
                createdCategory
            );
        }
    }
}
