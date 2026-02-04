using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Entities;
namespace Galaxium.Api.Services.Interfaces
{
    public interface IStockEntryService
    {
        Task<IEnumerable<StockEntry>> GetStockEntriesAsync();
        Task<StockEntry?> GetByIdStockEntryAsync(int id);
        Task<StockEntry> CreateStockEntryAsync(StockEntry stockEntry); //para ingresar stok de los productos que ya cree 
    }
}