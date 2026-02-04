using Galaxium.Api.Entities;

namespace Galaxium.Api.Repository.Interfaces
{
    public interface IStockEntryRepository
    {
        Task<IEnumerable<StockEntry>> GetStockEntriesAsync();
        Task<StockEntry?> GetByIdStockEntryAsync(int id);
        Task<StockEntry> CreateStockEntryAsync(StockEntry stockEntry); //para ingresar stok de los productos que ya cree 
    }
}
