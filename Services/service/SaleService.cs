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

        public SaleService(
            ISaleRepository saleRepository,
            SaleRules saleRules,
            IProductRepository productRepository,
            IEmailService emailService,
            ICustomerRepository customerRepository,
            IUserRepository userRepository
        )
        {
            _saleRepository = saleRepository;
            _saleRules = saleRules;
            _productRepository = productRepository;
            _emailService = emailService;
            _customerRepository = customerRepository;
            _userRepository = userRepository;
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
            // 1️⃣ Validaciones generales
            _saleRules.ValidateHasProducts(saleDetails.Count());
            _saleRules.ValidateSeller(sale.UserId);
            _saleRules.ValidatePaymentMethod(sale.PaymentMethodId);

            //  Validar cada detalle de producto y cargar Product desde DB
            foreach (var detail in saleDetails)
            {
                // Traer producto desde repositorio
                var product = await _productRepository.GetProductByIdAsync(detail.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Producto Id {detail.ProductId} no existe.");

                // no asigno detail.Product para evitar conflictos en EF Core tracking

                // ⚠️ ADVERTENCIA: Producto inactivo en lugar de excepción
                if (!product.IsActive)
                {
                    Console.WriteLine($"⚠️ ADVERTENCIA: El producto '{product.Name}' (Id: {product.Id}) está inactivo pero se permitirá la venta.");
                }

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

            // calcula totales de la venta
            // Calculamos SubTotal manualmente antes de guardar
            sale.SubTotal = saleDetails.Sum(d => d.Quantity * d.UnitPrice);
            _saleRules.ValidateDiscount(sale.Discount);
            _saleRules.ValidateDiscountLimit(sale.Discount, sale.SubTotal);
            sale.Total = _saleRules.CalculateTotal(sale.SubTotal, sale.Discount);

            _saleRules.ValidateTotal(sale.Total);

            // 5️⃣ Validar AmountPaid y calcular ChangeAmount
            _saleRules.ValidateAmountPaid(sale.AmountPaid, sale.Total, sale.PaymentMethodId);
            
            // Si el método de pago es efectivo (ID = 1), calcular el cambio
            if (sale.PaymentMethodId == 1 && sale.AmountPaid > 0)
            {
                sale.ChangeAmount = _saleRules.CalculateChange(sale.AmountPaid, sale.Total);
            }
            else
            {
                // Para otros métodos de pago, AmountPaid y ChangeAmount son 0
                sale.AmountPaid = 0;
                sale.ChangeAmount = 0;
            }

            sale.InvoiceNumber = _saleRules.GenerateInvoiceNumber();
            sale.SaleDate = DateTime.UtcNow;
            sale.Status = "COMPLETED";

            // 4️⃣ Delegar al repository la creación y manejo de stock
            var ventaCreada = await _saleRepository.CreateSaleWithDetailsAsync(sale, saleDetails);

            // 5️⃣ Enviar email de confirmación de compra (no crítico, se ejecuta pero no falla la transacción)
            try
            {
                // Obtener datos del cliente
                if (ventaCreada.CustomerId.HasValue)
                {
                    var cliente = await _customerRepository.GetByIdCustomerAsync(ventaCreada.CustomerId.Value);
                    if (cliente != null && !string.IsNullOrEmpty(cliente.Email))
                    {
                        // Obtener datos del vendedor
                        var vendedor = await _userRepository.GetUserByIdAsync(ventaCreada.UserId);
                        var nombreVendedor = vendedor?.FullName ?? "Vendedor";

                        // Cargar los detalles completos con información del producto
                        var detallesConProducto = new List<SaleDetail>();
                        foreach (var detalle in saleDetails)
                        {
                            var producto = await _productRepository.GetProductByIdAsync(detalle.ProductId);
                            detalle.Product = producto;
                            detallesConProducto.Add(detalle);
                        }

                        // Asignar PaymentMethod desde la venta
                        if (ventaCreada.PaymentMethod == null)
                        {
                            ventaCreada.PaymentMethod = new PaymentMethod { Name = "Efectivo" };
                        }

                        // Enviar email
                        await _emailService.EnviarEmailCompraBienvenida(
                            cliente.Email,
                            cliente.FullName,
                            ventaCreada,
                            detallesConProducto,
                            nombreVendedor
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // Loguear el error de email pero no fallar la venta
                Console.WriteLine($"⚠️ Error al enviar email de compra: {ex.Message}");
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
