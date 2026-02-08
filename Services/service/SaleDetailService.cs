using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Services.Rules;

namespace Galaxium.Api.Services.Implementations
{
    public class SaleDetailService : ISaleDetailService
    {
        private readonly ISaleDetailRepository _saleDetailRepository;
        private readonly SaleDetailsRules _saleDetailRules;

        public SaleDetailService(
            ISaleDetailRepository saleDetailRepository,
            SaleDetailsRules saleDetailRules
        )
        {
            _saleDetailRepository = saleDetailRepository;
            _saleDetailRules = saleDetailRules;
        }

        // ============================================
        // Obtener detalles por Id de venta
        // ============================================
        public async Task<IEnumerable<SaleDetail>> GetDetailsBySaleIdAsync(int saleId)
        {
            if (saleId <= 0)
                throw new ArgumentException("El Id de la venta debe ser mayor a cero.");

            var details = await _saleDetailRepository.GetBySaleIdAsync(saleId);

            if (details == null || !details.Any())
                throw new InvalidOperationException($"No se encontraron detalles para la venta Id {saleId}.");

            // Validar cada detalle según reglas de negocio
            foreach (var detail in details)
            {
                _saleDetailRules.ValidateProductIsActive(detail.Product.IsActive);
                _saleDetailRules.ValidateProductHasPrice(detail.Product.SalePrice);
                _saleDetailRules.ValidateQuantity(detail.Quantity);
                _saleDetailRules.ValidateQuantityAgainstStock(detail.Quantity, detail.Product.Stock);

                // Asignar UnitPrice y UnitCost si no están ya
                detail.UnitPrice = detail.Product.SalePrice ?? 0m;
                detail.UnitCost = detail.Product.CostPrice ?? 0m;
            }

            return details;
        }

        // ============================================
        // Obtener detalles por Id de producto
        // ============================================
        public async Task<IEnumerable<SaleDetail>> GetDetailsByProductIdAsync(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("El Id del producto debe ser mayor a cero.");

            var details = await _saleDetailRepository.GetByProductIdAsync(productId);

            if (details == null || !details.Any())
                throw new InvalidOperationException($"No se encontraron detalles para el producto Id {productId}.");

            // Validar cada detalle según reglas de negocio
            foreach (var detail in details)
            {
                _saleDetailRules.ValidateProductIsActive(detail.Product.IsActive);
                _saleDetailRules.ValidateProductHasPrice(detail.Product.SalePrice);
                _saleDetailRules.ValidateQuantity(detail.Quantity);
                _saleDetailRules.ValidateQuantityAgainstStock(detail.Quantity, detail.Product.Stock);

                // Asignar UnitPrice y UnitCost si no están ya
                detail.UnitPrice = detail.Product.SalePrice ?? 0m;
                detail.UnitCost = detail.Product.CostPrice ?? 0m;
            }

            return details;
        }
    }
}
