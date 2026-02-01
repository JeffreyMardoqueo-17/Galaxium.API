using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductPhotoController : ControllerBase
    {
        private readonly IProductPhotoService _photoService;

        public ProductPhotoController(IProductPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpPost("{productId}/photos")]
        public async Task<IActionResult> Upload(
            int productId,
            IFormFile file,
            bool isPrimary = false)
        {
            await _photoService.UploadAsync(productId, file, isPrimary);
            return Ok("Imagen subida correctamente");
        }
    }

}