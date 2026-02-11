using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Common;
using Galaxium.API.Entities;
using Galaxium.API.Models;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.Api.Services.service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ISkuGenerator _skuGenerator;
        private readonly IProductFilterRepository _productFilterRepository;

        public ProductService(
            IProductRepository productRepository,
            ISkuGenerator skuGenerator,
            IProductFilterRepository productFilterRepository)
        {
            _productRepository = productRepository;
            _skuGenerator = skuGenerator;
            _productFilterRepository = productFilterRepository;
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
        public async Task<Product> AddProductAsync(Product newProduct, int userId)
        {
            if (newProduct == null)
                throw new ArgumentNullException(nameof(newProduct));

            if (newProduct.CategoryId <= 0)
                throw new BusinessException("Category is required to generate SKU.");

            // 🔒 Estado inicial CONTROLADO
            newProduct.CreatedByUserId = userId;
            newProduct.Stock = 0;
            newProduct.CostPrice = null;
            newProduct.SalePrice = null;
            newProduct.IsActive = false;
            newProduct.CreatedAt = DateTime.UtcNow;

            // 🧠 Regla de negocio
            newProduct.SKU = await _skuGenerator.GenerateAsync(newProduct.CategoryId);

            return await _productRepository.AddProductAsync(newProduct);
        }



        public async Task<IEnumerable<Product>> GetProductsFilterAsync(ProductFilterModel filter)
        {
            // 🧠 Reglas de negocio 
            if (filter.PageSize > 100)
                filter.PageSize = 100;

            if (filter.Page < 1)
                filter.Page = 1;

            if (string.IsNullOrWhiteSpace(filter.OrderBy))
                filter.OrderBy = "CreatedAt";

            return await _productFilterRepository.GetProductsFilterAsync(filter);
        }

        //aqui falta que valide que si el stock es menor a 0 o es cero no puede actuzliarce el precio dle producto porque necesita que ya haya un stokc para saber el rpeico en el que se compro el producto 
        public async Task<Product?> UpdateProductPriceAsync(int productId, decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("El precio no puede ser negativo.");

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
                return null; // Producto no encontrado

            // Validación de stock: no se puede asignar precio si no hay stock
            if (product.Stock == null || product.Stock <= 0)
                throw new InvalidOperationException(
                    "No se puede asignar precio a un producto sin stock. Por favor registra primero la compra.");

            // Asignar o actualizar precio
            product.SalePrice = newPrice;

            // Activar si el precio es mayor a 0
            product.IsActive = newPrice > 0;

            // Guardar cambios en el repositorio
            return await _productRepository.UpdateProductPriceAsync(product);
        }
        public async Task<IEnumerable<Product>> GetProductsWithPhotosAsync()
        {
            var products = await _productRepository.GetProductsWithPhotosAsync();

            if (products == null || !products.Any())
                return Enumerable.Empty<Product>();

            foreach (var product in products)
            {
                if (product.Photos == null)
                    product.Photos = new List<ProductPhoto>();
            }

            return products;
        }

    }
}