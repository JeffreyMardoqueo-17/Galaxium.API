using Galaxium.Api.Entities;

namespace Galaxium.Api.Services.Interfaces;

public interface IStockAlertService
{
    Task<IReadOnlyList<StockAlert>> RefreshAlertsAsync();
    Task<IReadOnlyList<StockAlert>> GetActiveAlertsAsync();
    Task<StockAlert?> ResolveAlertAsync(int alertId);
}
