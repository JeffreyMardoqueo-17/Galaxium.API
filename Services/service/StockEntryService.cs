    using Galaxium.Api.Entities;
    using Galaxium.Api.Enums;
    using Galaxium.Api.Repository.Interfaces;
    using Galaxium.Api.Services.Interfaces;
    using Galaxium.API.Repository.Interfaces;

    namespace Galaxium.Api.Services
    {
        public class StockEntryService : IStockEntryService
        {
            private readonly IStockEntryRepository _stockEntryRepository;
            private readonly IProductRepository _productRepository;

            public StockEntryService(
                IStockEntryRepository stockEntryRepository,
                IProductRepository productRepository)
            {
                _stockEntryRepository = stockEntryRepository
                    ?? throw new ArgumentNullException(nameof(stockEntryRepository));
                _productRepository = productRepository
                    ?? throw new ArgumentNullException(nameof(productRepository));
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

                if (stockEntry.ProductId <= 0)
                    throw new InvalidOperationException("Producto inválido.");

                if (!stockEntry.UnitCost.HasValue || stockEntry.UnitCost <= 0)
                    throw new InvalidOperationException("El costo unitario es obligatorio.");

                // 1️⃣ Obtener producto
                var product = await _productRepository.GetProductByIdAsync(stockEntry.ProductId);
                if (product == null)
                    throw new InvalidOperationException("Producto no encontrado.");

                // 2️⃣ Validar ReferenceType y Quantity
                ValidateQuantity(stockEntry.ReferenceType, stockEntry.Quantity);

                // 3️⃣ Completar StockEntry
                stockEntry.CreatedAt = DateTime.UtcNow;
                stockEntry.RemainingQuantity = stockEntry.Quantity;


                // 4️⃣ Actualizar stock y reglas de negocio del producto
                switch (stockEntry.ReferenceType)
                {
                    case StockReferenceType.Purchase:
                        product.Stock += stockEntry.Quantity;
                        product.CostPrice = stockEntry.UnitCost.Value;
                        break;

                    case StockReferenceType.Sale:
                        if (product.Stock + stockEntry.Quantity < 0)
                            throw new InvalidOperationException("Stock insuficiente para realizar la venta.");
                        product.Stock += stockEntry.Quantity; // Quantity debe ser negativa
                        break;

                    case StockReferenceType.Adjustment:
                        product.Stock += stockEntry.Quantity;
                        break;

                    default:
                        throw new InvalidOperationException("Tipo de movimiento inválido.");
                }

                // Activación solo si tiene precio de venta
                product.IsActive = product.SalePrice.HasValue && product.SalePrice > 0;
                
                // 5️⃣ Persistencia
                return await _stockEntryRepository.CreateStockEntryAsync(stockEntry, product);
            }

            // 🔥 Validación centralizada
            private static void ValidateQuantity(StockReferenceType type, int quantity)
            {
                switch (type)
                {
                    case StockReferenceType.Purchase:
                        if (quantity <= 0)
                            throw new InvalidOperationException("Purchase requiere quantity positiva.");
                        break;

                    case StockReferenceType.Sale:
                        if (quantity >= 0)
                            throw new InvalidOperationException("Sale requiere quantity negativa.");
                        break;

                    case StockReferenceType.Adjustment:
                        if (quantity == 0)
                            throw new InvalidOperationException("Adjustment no puede ser cero.");
                        break;

                    default:
                        throw new InvalidOperationException("Tipo de movimiento inválido.");
                }
            }
        }
    }
