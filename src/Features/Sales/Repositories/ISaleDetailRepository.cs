using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.Api.Repository.Interfaces
{
   public interface ISaleDetailRepository
    {
        Task AddRangeAsync(IEnumerable<SaleDetail> details);

        Task<IEnumerable<SaleDetail>> GetBySaleIdAsync(int saleId);

        Task<IEnumerable<SaleDetail>> GetByProductIdAsync(int productId);

        Task SaveChangesAsync();
    }
}