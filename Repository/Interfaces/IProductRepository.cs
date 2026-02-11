using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.API.Repository.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<Product?> AddProductAsync(Product newProduct);
        Task<int> GetLastSkuNumberByCategoryAsync(int categoryId);
        //metodo para asignar o actualizar el precio de un producto
        Task<Product?> UpdateProductPriceAsync(Product product);

        //metodo para traer las fotos 
        Task<IEnumerable<Product>> GetProductsWithPhotosAsync();

    }
}