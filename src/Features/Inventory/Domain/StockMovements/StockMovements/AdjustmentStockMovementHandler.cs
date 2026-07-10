using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.service.StockMovements
{
    public class AdjustmentStockMovementHandler : IStockMovementHandler
    {
        public StockReferenceType ReferenceType => StockReferenceType.Adjustment;

        public void Apply(StockEntry stockEntry, Product product)
        {
            if (stockEntry.Quantity == 0)
            {
                throw new InvalidOperationException("Adjustment no puede ser cero.");
            }

            product.Stock += stockEntry.Quantity;
        }
    }
}
