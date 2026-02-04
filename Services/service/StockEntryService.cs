using Galaxium.Api.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;

namespace Galaxium.Api.Services
{
    public class StockEntryService : IStockEntryService
    {
        private readonly IStockEntryRepository _stockEntryRepository;

        public StockEntryService(IStockEntryRepository stockEntryRepository)
        {
            _stockEntryRepository = stockEntryRepository 
                ?? throw new ArgumentNullException(nameof(stockEntryRepository));
        }

        public async Task<IEnumerable<StockEntry>> GetStockEntriesAsync()
        {
            return await _stockEntryRepository.GetStockEntriesAsync();
        }

        public async Task<StockEntry> GetByIdStockEntryAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El id debe ser mayor que cero.");

            var stockEntry = await _stockEntryRepository.GetByIdStockEntryAsync(id);

            if (stockEntry == null)
                throw new KeyNotFoundException($"No se encontró la entrada de stock con id {id}");

            return stockEntry;
        }

        public async Task<StockEntry> CreateStockEntryAsync(StockEntry stockEntry)
        {
            if (stockEntry == null)
                throw new ArgumentNullException(nameof(stockEntry));

            if (stockEntry.Quantity <= 0)
                throw new InvalidOperationException("La cantidad de entrada debe ser mayor que cero.");

            stockEntry.CreatedAt = DateTime.UtcNow;

            return await _stockEntryRepository.CreateStockEntryAsync(stockEntry);
        }
    }
}
