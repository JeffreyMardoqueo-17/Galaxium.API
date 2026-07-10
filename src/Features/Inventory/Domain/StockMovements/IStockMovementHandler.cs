using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.Interfaces
{
    public interface IStockMovementHandler
    {
        StockReferenceType ReferenceType { get; }
        void Apply(StockEntry stockEntry, Product product);
    }
}
