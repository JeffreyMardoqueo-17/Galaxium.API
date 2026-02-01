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
        public ProductCategoryService(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
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
            if (_productCategoryRepository == null)
                throw new NotFoundBusinessException("ProductCategory repository is not available.");
                // Si no viene fecha, asignar ahora truncado a minutos
                var now = DateTime.Now;
                productCategory.CreatedAt = new DateTime(
                    now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            return await _productCategoryRepository.CreateProductCategory(productCategory);
        }
    }
}