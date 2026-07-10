using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.Api.Repository.Interfaces
{
   public interface ISaleRepository
{
    Task<Sale> CreateSaleWithDetailsAsync(Sale sale, IEnumerable<SaleDetail> details);
    Task<Sale?> GetByIdAsync(int saleId);
    Task<IEnumerable<Sale>> GetAllAsync();
    Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId);
}

}