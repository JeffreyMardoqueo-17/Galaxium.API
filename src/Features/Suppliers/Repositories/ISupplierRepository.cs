using Galaxium.Api.Entities;

namespace Galaxium.Api.Repository.Interfaces;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int supplierId);
    Task<Supplier> AddAsync(Supplier supplier);
    Task<Supplier?> UpdateAsync(Supplier supplier);
}
