using Galaxium.Api.Entities;

namespace Galaxium.Api.Services.Interfaces;

public interface IPurchaseService
{
    Task<Purchase> CreateAsync(int userId, int supplierId, IEnumerable<(int ProductId, int Quantity, decimal UnitPrice)> details);
    Task<IEnumerable<Purchase>> GetAllAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Purchase?> GetByIdAsync(int purchaseId);
}
