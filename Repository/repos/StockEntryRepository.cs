using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Data;
using Galaxium.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Repository.repos
{
    public class StockEntryRepository : IStockEntryRepository
    {
        private readonly GalaxiumDbContext _context;
        public StockEntryRepository(GalaxiumDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<StockEntry>> GetStockEntriesAsync()
        {
            return await _context.StockEntry
                .AsNoTracking()
                .Include(se => se.Product)
                .Include(se => se.User)
                .ToListAsync();
        }

        public async Task<StockEntry?> GetByIdStockEntryAsync(int id)
        {
            return await _context.StockEntry
                .AsNoTracking()
                .Include(se => se.Product)
                .Include(se => se.User)
                .FirstOrDefaultAsync(se => se.Id == id);
        }
        public async Task<StockEntry> CreateStockEntryAsync(StockEntry stockEntry)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Obtener producto primero
                var product = await _context.Product
                    .FirstOrDefaultAsync(p => p.Id == stockEntry.ProductId);

                if (product == null)
                    throw new Exception("Producto no encontrado");

                // // 2️⃣ Calcular TotalCost en C# para evitar OUTPUT
                // stockEntry.TotalCost = stockEntry.Quantity * stockEntry.UnitCost;
                // stockEntry.CreatedAt = DateTime.UtcNow;
                // stockEntry.IsActive = true;

                // // 3️⃣ Insertar entrada de stock
                // _context.StockEntry.Add(stockEntry);

                // 4️⃣ Actualizar stock del producto
                product.Stock += stockEntry.Quantity;

                // 5️⃣ Registrar movimiento de stock
                var movement = new StockMovement
                {
                    ProductId = stockEntry.ProductId,
                    UserId = stockEntry.UserId,
                    MovementType = "IN",
                    Quantity = stockEntry.Quantity,
                    Reference = "PURCHASE",
                    CreatedAt = DateTime.UtcNow
                };
                _context.StockMovement.Add(movement);

                // 6️⃣ Guardar todo de una vez
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 7️⃣ Retornar entrada con relaciones
                return await _context.StockEntry
                    .AsNoTracking()
                    .Include(se => se.Product)
                    .Include(se => se.User)
                    .FirstAsync(se => se.Id == stockEntry.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}