using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Services.Rules;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.Api.Services.Implementations
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly SaleRules _saleRules;
        private readonly IProductRepository _productRepository;
        public SaleService(
            ISaleRepository saleRepository,
            SaleRules saleRules,
            IProductRepository productRepository
        )
        {
            _saleRepository = saleRepository;
            _saleRules = saleRules;
            _productRepository = productRepository;
        }
        // ============================================
        // Crear venta completa (cabecera + detalles)
        //         {
        //   "customerId": 1,
        //   "paymentMethodId": 1,
        //   "discount": 0,
        //   "details": [
        //     {
        //       "productId": 8,
        //       "quantity": 1
        //     }
        //   ]
        // }

        // ============================================
        public async Task<Sale> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> saleDetails)
        {
            // 1️⃣ Validaciones generales
            _saleRules.ValidateHasProducts(saleDetails.Count());
            _saleRules.ValidateSeller(sale.UserId);
            _saleRules.ValidatePaymentMethod(sale.PaymentMethodId);

            // 2️⃣ Validar cada detalle de producto y cargar Product desde DB
            foreach (var detail in saleDetails)
            {
                // Traer producto desde repositorio
                var product = await _productRepository.GetProductByIdAsync(detail.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Producto Id {detail.ProductId} no existe.");

                // ⚠️ NO asignamos detail.Product para evitar conflictos en EF Core tracking

                // Validaciones
                if (!product.IsActive)
                    throw new InvalidOperationException($"El producto '{product.Name}' está inactivo.");

                if (!product.SalePrice.HasValue || product.SalePrice.Value <= 0)
                    throw new InvalidOperationException($"El producto '{product.Name}' no tiene precio asignado.");

                if (detail.Quantity <= 0)
                    throw new InvalidOperationException($"La cantidad del producto '{product.Name}' debe ser mayor a cero.");

                if (detail.Quantity > product.Stock)
                    throw new InvalidOperationException($"No hay suficiente stock para el producto '{product.Name}'. Stock disponible: {product.Stock}");

                // Asignar precios (SubTotal lo calcula SQL Server automáticamente)
                detail.UnitPrice = product.SalePrice.Value;
                detail.UnitCost = product.CostPrice ?? 0m;
            }

            // 3️⃣ Calcular totales de la venta
            // Calculamos SubTotal manualmente antes de guardar
            sale.SubTotal = saleDetails.Sum(d => d.Quantity * d.UnitPrice);
            _saleRules.ValidateDiscount(sale.Discount);
            _saleRules.ValidateDiscountLimit(sale.Discount, sale.SubTotal);
            sale.Total = _saleRules.CalculateTotal(sale.SubTotal, sale.Discount);

            _saleRules.ValidateTotal(sale.Total);

            sale.InvoiceNumber = _saleRules.GenerateInvoiceNumber();
            sale.SaleDate = DateTime.UtcNow;
            sale.Status = "COMPLETED";

            // 4️⃣ Delegar al repository la creación y manejo de stock
            return await _saleRepository.CreateSaleWithDetailsAsync(sale, saleDetails);
        }

        // ============================================
        // Consultas
        // ============================================
        public async Task<Sale?> GetSaleByIdAsync(int saleId)
        {
            // Validación de parámetro
            if (saleId <= 0)
                throw new ArgumentException("El Id de la venta debe ser mayor a cero.");

            // Consulta al repositorio
            var sale = await _saleRepository.GetByIdAsync(saleId);

            // Validación de existencia
            if (sale == null)
                throw new InvalidOperationException($"No se encontró ninguna venta con Id {saleId}.");

            return sale;
        }


        public async Task<IEnumerable<Sale>> GetAllSalesAsync()
        {
            var sales = await _saleRepository.GetAllAsync();

            return sales ?? Enumerable.Empty<Sale>();
        }


        public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime start, DateTime end)
        {
            // Validaciones
            if (start > end)
                throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha final.");

            if ((end - start).TotalDays > 3650) // 10 años máximo
                throw new InvalidOperationException("El rango de fechas es demasiado grande.");

            var sales = await _saleRepository.GetByDateRangeAsync(start, end);

            if (sales == null || !sales.Any())
                throw new InvalidOperationException("No se encontraron ventas en el rango especificado.");

            return sales;
        }


        public async Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("El Id del cliente debe ser mayor a cero.");

            var sales = await _saleRepository.GetByCustomerAsync(customerId);

            if (sales == null || !sales.Any())
                throw new InvalidOperationException($"No se encontraron ventas para el cliente Id {customerId}.");

            return sales;
        }

    }
}
