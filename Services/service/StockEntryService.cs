using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Services.Rules;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.Api.Services
{
    public class StockEntryService : IStockEntryService
    {
        private readonly IStockEntryRepository _stockEntryRepository;
        private readonly IProductRepository _productRepository;
        private readonly StockEntryRules _rules;

        public StockEntryService(
            IStockEntryRepository stockEntryRepository,
            IProductRepository productRepository,
            StockEntryRules rules)
        {
            _stockEntryRepository = stockEntryRepository
                ?? throw new ArgumentNullException(nameof(stockEntryRepository));

            _productRepository = productRepository
                ?? throw new ArgumentNullException(nameof(productRepository));

            _rules = rules
                ?? throw new ArgumentNullException(nameof(rules));
        }

        // ===============================
        // GET ALL
        // ===============================
        public async Task<IEnumerable<StockEntry>> GetStockEntriesAsync()
        {
            return await _stockEntryRepository.GetStockEntriesAsync();
        }

        // ===============================
        // GET LAST BY PRODUCT
        // ===============================
        public async Task<StockEntry?> GetLastEntryByProductIdAsync(int productId)
        {
            return await _stockEntryRepository
                .GetLastEntryByProductIdAsync(productId);
        }

        // ===============================
        // GET BY ID
        // ===============================
        public async Task<StockEntry> GetByIdStockEntryAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException(
                    "El id debe ser mayor que cero.");

            var stockEntry =
                await _stockEntryRepository
                    .GetByIdStockEntryAsync(id);

            if (stockEntry == null)
                throw new KeyNotFoundException(
                    $"No se encontró la entrada de stock con id {id}");

            return stockEntry;
        }

        // ===============================
        // CREATE
        // ===============================
        public async Task<StockEntry> CreateStockEntryAsync(
            StockEntry stockEntry)
        {
            if (stockEntry == null)
                throw new ArgumentNullException(nameof(stockEntry));

            // ===============================
            // 1️⃣ Validaciones base
            // ===============================
            _rules.ValidateQuantity(
                Math.Abs(stockEntry.Quantity));

            if (stockEntry.UnitCost <= 0)
                throw new InvalidOperationException(
                    "El costo unitario debe ser mayor que cero.");

            _rules.ValidateUnitCost(stockEntry.UnitCost);
            _rules.ValidateUser(stockEntry.UserId);

            // ===============================
            // 2️⃣ Obtener producto
            // ===============================
            var product =
                await _productRepository
                    .GetProductByIdAsync(
                        stockEntry.ProductId);

            _rules.ValidateProductExists(product);

            // ===============================
            // 3️⃣ Validar lógica por tipo
            // ===============================
            ValidateQuantity(
                stockEntry.ReferenceType,
                stockEntry.Quantity);

            // ===============================
            // 4️⃣ Último lote
            // ===============================
            var lastEntry =
                await _stockEntryRepository
                    .GetLastEntryByProductIdAsync(
                        stockEntry.ProductId);

            if (lastEntry != null)
            {
                _rules.ValidateDuplicateEntry(
                    Math.Abs(stockEntry.Quantity),
                    stockEntry.UnitCost,
                    lastEntry.CreatedAt);

                _rules.ValidateCostVariation(
                    stockEntry.UnitCost,
                    lastEntry.UnitCost);
            }

            // ===============================
            // 5️⃣ Datos del lote
            // ===============================
            stockEntry.CreatedAt = DateTime.UtcNow;

            stockEntry.RemainingQuantity =
                _rules.InitializeRemaining(
                    Math.Abs(stockEntry.Quantity));

            // ===============================
            // 6️⃣ Reglas sobre producto
            // ===============================
            switch (stockEntry.ReferenceType)
            {
                case StockReferenceType.Purchase:

                    _rules.ValidateExtremeQuantity(
                        stockEntry.Quantity);

                    product.Stock +=
                        stockEntry.Quantity;

                    // Último costo de compra
                    product.CostPrice =
                        stockEntry.UnitCost;

                    break;

                case StockReferenceType.Sale:

                    if (product.Stock +
                        stockEntry.Quantity < 0)
                    {
                        throw new InvalidOperationException(
                            "Stock insuficiente para realizar la venta.");
                    }

                    product.Stock +=
                        stockEntry.Quantity;

                    break;

                case StockReferenceType.Adjustment:

                    product.Stock +=
                        stockEntry.Quantity;

                    break;

                default:
                    throw new InvalidOperationException(
                        "Tipo de movimiento inválido.");
            }

            // ===============================
            // 7️⃣ Activación producto
            // ===============================
            product.IsActive =
                product.SalePrice.HasValue &&
                product.SalePrice > 0;

            // ===============================
            // 8️⃣ Persistencia
            // ===============================
            return await _stockEntryRepository
                .CreateStockEntryAsync(
                    stockEntry,
                    product);
        }

        // ===============================
        // VALIDACIÓN CENTRAL
        // ===============================
        private static void ValidateQuantity(
            StockReferenceType type,
            int quantity)
        {
            switch (type)
            {
                case StockReferenceType.Purchase:

                    if (quantity <= 0)
                        throw new InvalidOperationException(
                            "Purchase requiere quantity positiva.");

                    break;

                case StockReferenceType.Sale:

                    if (quantity >= 0)
                        throw new InvalidOperationException(
                            "Sale requiere quantity negativa.");

                    break;

                case StockReferenceType.Adjustment:

                    if (quantity == 0)
                        throw new InvalidOperationException(
                            "Adjustment no puede ser cero.");

                    break;

                default:
                    throw new InvalidOperationException(
                        "Tipo de movimiento inválido.");
            }
        }
    }
}
