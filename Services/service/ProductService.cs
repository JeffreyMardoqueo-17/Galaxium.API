using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Common;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.Api.Services.service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ISkuGenerator _skuGenerator;

        public ProductService(
            IProductRepository productRepository,
            ISkuGenerator skuGenerator)
        {
            _productRepository = productRepository;
            _skuGenerator = skuGenerator;
        }


        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            var produts = await _productRepository.GetProductsAsync();
            if (!produts.Any())
            {
                return Enumerable.Empty<Product>();
                throw new NotFoundBusinessException("No products found.");
            }
            return produts;
        }
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("Invalid product ID.");
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundBusinessException("Product not found.");
            }
            return product;
        }
        public async Task<Product?> AddProductAsync(Product newProduct)
        {
            if (newProduct == null)
                throw new ArgumentException("Product cannot be null.");

            if (newProduct.CategoryId <= 0)
                throw new BusinessException("Category is required to generate SKU.");

            // 🔹 Generar SKU en backend (regla de negocio)
            newProduct.SKU = await _skuGenerator.GenerateAsync(newProduct.CategoryId);

            var addedProduct = await _productRepository.AddProductAsync(newProduct);
            return addedProduct;
        }

    }
}