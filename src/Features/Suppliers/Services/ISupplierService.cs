using Galaxium.Api.Entities;

namespace Galaxium.Api.Services.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int supplierId);
    Task<Supplier> AddAsync(Supplier supplier);
    Task<Supplier> UpdateAsync(int supplierId, Supplier supplier);
}
