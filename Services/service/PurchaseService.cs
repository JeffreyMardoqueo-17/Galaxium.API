using Galaxium.Api.Entities;
using Galaxium.Api.Enums;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.Api.Services.service;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockEntryRepository _stockEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockAlertService _stockAlertService;

    public PurchaseService(
        IPurchaseRepository purchaseRepository,
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IStockEntryRepository stockEntryRepository,
        IUnitOfWork unitOfWork,
        IStockAlertService stockAlertService)
    {
        _purchaseRepository = purchaseRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _stockEntryRepository = stockEntryRepository;
        _unitOfWork = unitOfWork;
        _stockAlertService = stockAlertService;
    }

    public async Task<Purchase> CreateAsync(int userId, int supplierId, IEnumerable<(int ProductId, int Quantity, decimal UnitPrice)> details)
    {
        if (userId <= 0)
            throw new ArgumentException("Usuario invalido.");

        var detailList = details.ToList();
        if (detailList.Count == 0)
            throw new InvalidOperationException("La compra debe incluir al menos un producto.");

        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null || !supplier.IsActive)
            throw new InvalidOperationException("Proveedor no encontrado o inactivo.");

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var purchase = new Purchase
            {
                SupplierId = supplierId,
                UserId = userId,
                PurchaseDate = DateTime.UtcNow,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow,
            };

            var mappedDetails = new List<PurchaseDetail>();
            decimal total = 0;

            foreach (var item in detailList)
            {
                if (item.Quantity <= 0)
                    throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

                if (item.UnitPrice <= 0)
                    throw new InvalidOperationException("El precio unitario debe ser mayor a cero.");

                var product = await _productRepository.GetProductByIdAsync(item.ProductId)
                    ?? throw new InvalidOperationException($"Producto {item.ProductId} no existe.");

                product.Stock += item.Quantity;
                product.CostPrice = item.UnitPrice;
                if (product.SalePrice.HasValue && product.SalePrice > 0)
                {
                    product.IsActive = true;
                }

                await _productRepository.UpdateProductPriceAsync(product);

                var lineTotal = item.Quantity * item.UnitPrice;
                total += lineTotal;

                mappedDetails.Add(new PurchaseDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = lineTotal
                });

                var stockEntry = new StockEntry
                {
                    ProductId = item.ProductId,
                    UserId = userId,
                    Quantity = item.Quantity,
                    RemainingQuantity = item.Quantity,
                    UnitCost = item.UnitPrice,
                    ReferenceType = StockReferenceType.Purchase,
                    SupplierId = supplierId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _stockEntryRepository.CreateStockEntryAsync(stockEntry, product);
            }

            purchase.Total = total;
            purchase.Details = mappedDetails;
            var createdPurchase = await _purchaseRepository.AddAsync(purchase);
            await _stockAlertService.RefreshAlertsAsync();
            return createdPurchase;
        });
    }

    public async Task<IEnumerable<Purchase>> GetAllAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _purchaseRepository.GetAllAsync(startDate, endDate);
    }

    public async Task<Purchase?> GetByIdAsync(int purchaseId)
    {
        if (purchaseId <= 0)
            throw new ArgumentException("PurchaseId invalido.");

        return await _purchaseRepository.GetByIdAsync(purchaseId);
    }
}
