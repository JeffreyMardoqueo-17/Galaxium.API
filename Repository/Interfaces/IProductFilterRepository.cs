using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;
using Galaxium.API.Models;

namespace Galaxium.API.Repository.Interfaces
{
    public interface IProductFilterRepository
    {
        Task<IEnumerable<Product>> GetProductsAsync(ProductFilterModel filter);
    }
}