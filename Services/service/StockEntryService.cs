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
        private readonly IStockMovementHandlerFactory _stockMovementHandlerFactory;
        private readonly IUnitOfWork _unitOfWork;

        public StockEntryService(
            IStockEntryRepository stockEntryRepository,
            IProductRepository productRepository,
            StockEntryRules rules,
            IStockMovementHandlerFactory stockMovementHandlerFactory,
            IUnitOfWork unitOfWork)
        {
            _stockEntryRepository = stockEntryRepository
                ?? throw new ArgumentNullException(nameof(stockEntryRepository));

            _productRepository = productRepository
                ?? throw new ArgumentNullException(nameof(productRepository));

            _rules = rules
                ?? throw new ArgumentNullException(nameof(rules));

            _stockMovementHandlerFactory = stockMovementHandlerFactory
                ?? throw new ArgumentNullException(nameof(stockMovementHandlerFactory));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
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

            if (product == null)
            {
                throw new InvalidOperationException("Producto no encontrado.");
            }

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
            // 6️⃣ Reglas por tipo usando Factory Method
            // ===============================
            if (stockEntry.ReferenceType == StockReferenceType.Purchase)
            {
                _rules.ValidateExtremeQuantity(stockEntry.Quantity);
            }

            var movementHandler =
                _stockMovementHandlerFactory.Create(stockEntry.ReferenceType);
            movementHandler.Apply(stockEntry, product);

            // ===============================
            // 7️⃣ Activación producto
            // ===============================
            product.IsActive =
                product.SalePrice.HasValue &&
                product.SalePrice > 0;

            // ===============================
            // 8️⃣ Persistencia
            // ===============================
            return await _unitOfWork.ExecuteInTransactionAsync(
                () => _stockEntryRepository.CreateStockEntryAsync(stockEntry, product));
        }
    }
}
