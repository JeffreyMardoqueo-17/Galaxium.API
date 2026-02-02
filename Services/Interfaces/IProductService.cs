using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;
using Galaxium.API.Models;

namespace Galaxium.Api.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<Product?> AddProductAsync(Product newProduct);
        Task<IEnumerable<Product>> GetProductsFilterAsync(ProductFilterModel filter);
    }
}