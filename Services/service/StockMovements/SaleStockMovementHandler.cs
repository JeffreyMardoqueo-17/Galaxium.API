using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.service.StockMovements
{
    public class SaleStockMovementHandler : IStockMovementHandler
    {
        public StockReferenceType ReferenceType => StockReferenceType.Sale;

        public void Apply(StockEntry stockEntry, Product product)
        {
            if (stockEntry.Quantity >= 0)
            {
                throw new InvalidOperationException("Sale requiere quantity negativa.");
            }

            if (product.Stock + stockEntry.Quantity < 0)
            {
                throw new InvalidOperationException("Stock insuficiente para realizar la venta.");
            }

            product.Stock += stockEntry.Quantity;
        }
    }
}
