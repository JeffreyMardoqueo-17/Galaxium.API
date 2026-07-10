using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.service.StockMovements
{
    public class PurchaseStockMovementHandler : IStockMovementHandler
    {
        public StockReferenceType ReferenceType => StockReferenceType.Purchase;

        public void Apply(StockEntry stockEntry, Product product)
        {
            if (stockEntry.Quantity <= 0)
            {
                throw new InvalidOperationException("Purchase requiere quantity positiva.");
            }

            product.Stock += stockEntry.Quantity;
            product.CostPrice = stockEntry.UnitCost;
        }
    }
}
