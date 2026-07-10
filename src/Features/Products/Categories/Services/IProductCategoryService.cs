using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.Interfaces
{
    public interface IProductCategoryService
    {
        
        Task<IEnumerable<ProductCategory>> GetAllProductCategories();
        Task<ProductCategory?> GetProductCategoryById(int id);
        Task<ProductCategory> CreateProductCategory(ProductCategory productCategory);
    }
}