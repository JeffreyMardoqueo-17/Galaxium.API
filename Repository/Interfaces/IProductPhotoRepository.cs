using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.API.Repository.Interfaces
{
    public interface IProductPhotoRepository
    {
         Task AddAsync(ProductPhoto photo);
    Task<int> CountByProductAsync(int productId);
    }
}