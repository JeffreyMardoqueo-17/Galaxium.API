using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.service.StockMovements
{
    public class ReturnStockMovementHandler : IStockMovementHandler
    {
        public StockReferenceType ReferenceType => StockReferenceType.Return;

        public void Apply(StockEntry stockEntry, Product product)
        {
            if (stockEntry.Quantity <= 0)
            {
                throw new InvalidOperationException("Return requiere quantity positiva.");
            }

            product.Stock += stockEntry.Quantity;
            if (product.Stock > 0 && product.SalePrice.HasValue && product.SalePrice > 0)
            {
                product.IsActive = true;
            }
        }
    }
}
