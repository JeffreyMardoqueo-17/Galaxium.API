using Galaxium.Api.Entities;
using Galaxium.API.Entities;

namespace Galaxium.Api.Repository.Interfaces
{
    public interface IStockEntryRepository
    {
        Task<IEnumerable<StockEntry>> GetStockEntriesAsync();
        Task<StockEntry?> GetByIdStockEntryAsync(int id);
         Task<StockEntry> CreateStockEntryAsync(
                                                                StockEntry stockEntry,
                                                                Product product
                                                            );
    }
}
