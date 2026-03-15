using Galaxium.Api.Enums;
using Galaxium.Api.Services.Interfaces;

namespace Galaxium.Api.Services.service.StockMovements
{
    public class StockMovementHandlerFactory : IStockMovementHandlerFactory
    {
        private readonly IReadOnlyDictionary<StockReferenceType, IStockMovementHandler> _handlers;

        public StockMovementHandlerFactory(IEnumerable<IStockMovementHandler> handlers)
        {
            _handlers = handlers.ToDictionary(h => h.ReferenceType, h => h);
        }

        public IStockMovementHandler Create(StockReferenceType referenceType)
        {
            if (!_handlers.TryGetValue(referenceType, out var handler))
            {
                throw new InvalidOperationException("Tipo de movimiento invalido.");
            }

            return handler;
        }
    }
}
