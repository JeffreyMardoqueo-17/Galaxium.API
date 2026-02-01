using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Common;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.Api.Services.service
{
    public class SkuGenerator
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _categoryRepository;

        public SkuGenerator(
            IProductRepository productRepository,
            IProductCategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // public async Task<string> GenerateAsync(int categoryId)
        // {
        //     var category = await _categoryRepository.GetProductCategoryById(categoryId)
        //         ?? throw new BusinessException("Categoría no encontrada");

        //     var lastNumber = await _productRepository.GetLastSkuNumberByCategory(categoryId);

        //     var nextNumber = lastNumber + 1;

        //     return $"{category.Code}-{nextNumber:D3}";
        // }
    }


}