using Galaxium.Api.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;

namespace Galaxium.Api.Services.service;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierService(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _supplierRepository.GetAllAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int supplierId)
    {
        if (supplierId <= 0)
            throw new ArgumentException("SupplierId invalido.");

        return await _supplierRepository.GetByIdAsync(supplierId);
    }

    public async Task<Supplier> AddAsync(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new ArgumentException("El nombre del proveedor es obligatorio.");

        supplier.CreatedAt = DateTime.UtcNow;
        supplier.IsActive = true;

        return await _supplierRepository.AddAsync(supplier);
    }

    public async Task<Supplier> UpdateAsync(int supplierId, Supplier supplier)
    {
        if (supplierId <= 0)
            throw new ArgumentException("SupplierId invalido.");

        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new ArgumentException("El nombre del proveedor es obligatorio.");

        supplier.Id = supplierId;

        var updated = await _supplierRepository.UpdateAsync(supplier);
        if (updated == null)
            throw new KeyNotFoundException("Proveedor no encontrado.");

        return updated;
    }
}
