using Galaxium.Api.Enums;

namespace Galaxium.Api.Services.Interfaces
{
    public interface IStockMovementHandlerFactory
    {
        IStockMovementHandler Create(StockReferenceType referenceType);
    }
}
