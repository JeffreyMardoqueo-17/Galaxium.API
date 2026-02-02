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
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly ICategoryCodeGenerator _codeGenerator;

        public ProductCategoryService(
            IProductCategoryRepository productCategoryRepository,
            ICategoryCodeGenerator codeGenerator)
        {
            _productCategoryRepository = productCategoryRepository;
            _codeGenerator = codeGenerator;
        }
        public async Task<IEnumerable<ProductCategory>> GetAllProductCategories()
        {
            if (_productCategoryRepository == null)
                throw new NotFoundBusinessException("ProductCategory repository is not available.");
            return await _productCategoryRepository.GetAllProductCategories();
        }
        public async Task<ProductCategory?> GetProductCategoryById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("The provided ID is not valid.", nameof(id));
            if (_productCategoryRepository == null)
                throw new NotFoundBusinessException("ProductCategory repository is not available.");
            return await _productCategoryRepository.GetProductCategoryById(id);
        }
        public async Task<ProductCategory> CreateProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new BusinessException("ProductCategory cannot be null.");

            // 🔥 GENERAR CÓDIGO SI NO VIENE
            if (string.IsNullOrWhiteSpace(productCategory.Code))
            {
                productCategory.Code =
                    await _codeGenerator.GenerateAsync(productCategory.Name);
            }

            productCategory.CreatedAt = DateTime.Now;

            return await _productCategoryRepository.CreateProductCategory(productCategory);
        }
    }
}