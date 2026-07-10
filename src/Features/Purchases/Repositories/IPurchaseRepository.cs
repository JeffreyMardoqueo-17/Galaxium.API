using Galaxium.Api.Entities;

namespace Galaxium.Api.Repository.Interfaces;

public interface IPurchaseRepository
{
    Task<Purchase> AddAsync(Purchase purchase);
    Task<IEnumerable<Purchase>> GetAllAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Purchase?> GetByIdAsync(int purchaseId);
}
