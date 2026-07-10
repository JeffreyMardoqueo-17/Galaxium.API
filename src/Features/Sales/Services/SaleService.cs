using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Services.Rules;
using Galaxium.API.Repository.Interfaces;
using Galaxium.Api.Entities;
using Microsoft.Extensions.Logging;

namespace Galaxium.Api.Services.Implementations
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly SaleRules _saleRules;
        private readonly IProductRepository _productRepository;
        private readonly IEmailService _emailService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISalePdfService _salePdfService;
        private readonly IStockAlertService _stockAlertService;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly ILogger<SaleService> _logger;

        public SaleService(
            ISaleRepository saleRepository,
            SaleRules saleRules,
            IProductRepository productRepository,
            IEmailService emailService,
            ICustomerRepository customerRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ISalePdfService salePdfService,
            IStockAlertService stockAlertService,
            IPaymentMethodRepository paymentMethodRepository,
            ILogger<SaleService> logger
        )
        {
            _saleRepository = saleRepository;
            _saleRules = saleRules;
            _productRepository = productRepository;
            _emailService = emailService;
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _salePdfService = salePdfService;
            _stockAlertService = stockAlertService;
            _paymentMethodRepository = paymentMethodRepository;
            _logger = logger;
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

        // ============================================ aqui tengo qeu agregar cuanto dinero me dio cada cliente para calcular el vuelto
        public async Task<Sale> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> saleDetails)
        {
            // 1 Validación de método de pago activo
            var activePaymentMethods = await _paymentMethodRepository.GetAllAsync();
            if (!activePaymentMethods.Any())
                throw new InvalidOperationException("No hay métodos de pago activos en el sistema. Contacte a soporte o ejecute el script de inicialización correctamente.");

            var paymentMethod = activePaymentMethods.FirstOrDefault(pm => pm.Id == sale.PaymentMethodId);
            if (paymentMethod == null)
                throw new InvalidOperationException($"El método de pago seleccionado (ID: {sale.PaymentMethodId}) no está activo o no existe.");

            // 2️ Validaciones generales
            _saleRules.ValidateHasProducts(saleDetails.Count());
            _saleRules.ValidateSeller(sale.UserId);
            _saleRules.ValidatePaymentMethod(sale.PaymentMethodId);

            // 3️ Validar cada detalle de producto y cargar Product desde DB
            foreach (var detail in saleDetails)
            {
                var product = await _productRepository.GetProductByIdAsync(detail.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Producto Id {detail.ProductId} no existe.");

                if (!product.IsActive)
                    _logger.LogWarning("El producto {ProductName} (Id: {ProductId}) está inactivo pero se permitirá la venta.", product.Name, product.Id);

                if (!product.SalePrice.HasValue || product.SalePrice.Value <= 0)
                    throw new InvalidOperationException($"El producto '{product.Name}' no tiene precio asignado.");

                if (detail.Quantity <= 0)
                    throw new InvalidOperationException($"La cantidad del producto '{product.Name}' debe ser mayor a cero.");

                if (detail.Quantity > product.Stock)
                    throw new InvalidOperationException($"No hay suficiente stock para el producto '{product.Name}'. Stock disponible: {product.Stock}");

                detail.UnitPrice = product.SalePrice.Value;
                detail.UnitCost = product.CostPrice ?? 0m;
            }

            // 4️ Calcula totales de la venta
            sale.SubTotal = saleDetails.Sum(d => d.Quantity * d.UnitPrice);
            _saleRules.ValidateDiscount(sale.Discount, sale.IsDiscountPercentage);
            
            // Si es descuento porcentual, calcular el monto del descuento
            if (sale.IsDiscountPercentage)
            {
                sale.Discount = _saleRules.CalculateDiscountAmount(sale.Discount, sale.SubTotal);
            }
            
            _saleRules.ValidateDiscountLimit(sale.Discount, sale.SubTotal);
            sale.Total = _saleRules.CalculateTotal(sale.SubTotal, sale.Discount);
            _saleRules.ValidateTotal(sale.Total);

            // 5️⃣ Validar AmountPaid y calcular ChangeAmount
            _saleRules.ValidateAmountPaid(sale.AmountPaid, sale.Total, sale.PaymentMethodId);
            if (sale.PaymentMethodId == 1 && sale.AmountPaid > 0)
            {
                sale.ChangeAmount = _saleRules.CalculateChange(sale.AmountPaid, sale.Total);
            }
            else
            {
                sale.AmountPaid = 0;
                sale.ChangeAmount = 0;
            }

            sale.InvoiceNumber = _saleRules.GenerateInvoiceNumber();
            sale.SaleDate = DateTime.UtcNow;
            sale.Status = "COMPLETED";

            // 6️⃣ Ejecutar todo en transacción (UnitOfWork)
            Sale ventaCreada;
            try
            {
                ventaCreada = await _unitOfWork.ExecuteInTransactionAsync(
                    () => _saleRepository.CreateSaleWithDetailsAsync(sale, saleDetails));
            }
            catch (Exception ex)
            {
                // Loguear y lanzar error claro
                _logger.LogError(ex, "Error crítico al guardar la venta para el usuario {UserId}", sale.UserId);
                throw new InvalidOperationException("Ocurrió un error al guardar la venta. Ningún cambio fue aplicado. Detalle: " + ex.Message);
            }

            // 7️⃣ Envío de email y alertas fuera de la transacción
            try
            {
                if (ventaCreada.CustomerId.HasValue)
                {
                    var cliente = await _customerRepository.GetByIdCustomerAsync(ventaCreada.CustomerId.Value);
                    if (cliente != null && !string.IsNullOrEmpty(cliente.Email))
                    {
                        var vendedor = await _userRepository.GetUserByIdAsync(ventaCreada.UserId);
                        var nombreVendedor = vendedor?.FullName ?? "Vendedor";
                        var detallesConProducto = new List<SaleDetail>();
                        foreach (var detalle in saleDetails)
                        {
                            var producto = await _productRepository.GetProductByIdAsync(detalle.ProductId);
                            detallesConProducto.Add(new SaleDetail
                            {
                                ProductId = detalle.ProductId,
                                Quantity = detalle.Quantity,
                                UnitPrice = detalle.UnitPrice,
                                UnitCost = detalle.UnitCost,
                                Product = producto == null
                                    ? null
                                    : new Product
                                    {
                                        Id = producto.Id,
                                        Name = producto.Name
                                    }
                            });
                        }
                        if (ventaCreada.PaymentMethod == null)
                        {
                            ventaCreada.PaymentMethod = new PaymentMethod { Name = "Efectivo" };
                        }
                        byte[]? invoicePdfBytes = null;
                        string? invoiceFileName = null;

                        try
                        {
                            invoicePdfBytes = _salePdfService.GenerateInvoicePdf(ventaCreada);
                            invoiceFileName = $"Factura-{ventaCreada.InvoiceNumber ?? ventaCreada.Id.ToString()}.pdf";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "No se pudo generar el PDF de la factura para la venta {SaleId}. Se enviará el correo sin adjunto.",
                                ventaCreada.Id);
                        }

                        await _emailService.EnviarEmailCompraBienvenida(
                            cliente.Email,
                            cliente.FullName,
                            ventaCreada,
                            detallesConProducto,
                            nombreVendedor,
                            invoicePdfBytes,
                            invoiceFileName
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de compra para la venta {SaleId}", ventaCreada.Id);
            }

            try
            {
                await _stockAlertService.RefreshAlertsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al refrescar alertas de stock después de la venta {SaleId}", ventaCreada.Id);
            }

            return ventaCreada;
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

            return sales ?? Enumerable.Empty<Sale>();
        }


        public async Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("El Id del cliente debe ser mayor a cero.");

            var sales = await _saleRepository.GetByCustomerAsync(customerId);

            return sales ?? Enumerable.Empty<Sale>();
        }

    }
}
