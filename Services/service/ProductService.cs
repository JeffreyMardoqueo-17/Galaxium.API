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
    public class ProductService: IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
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
            var addedProduct = await _productRepository.AddProductAsync(newProduct);
            return addedProduct;
        }
    }
}