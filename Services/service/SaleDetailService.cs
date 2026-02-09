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

            // ⚠️ CONSULTA HISTÓRICA: No aplicar validaciones de negocio
            // Los detalles reflejan ventas ya realizadas, aunque el producto
            // ahora esté inactivo o sin stock
            
            return details ?? Enumerable.Empty<SaleDetail>();
        }

        // ============================================
        // Obtener detalles por Id de producto
        // ============================================
        public async Task<IEnumerable<SaleDetail>> GetDetailsByProductIdAsync(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("El Id del producto debe ser mayor a cero.");

            var details = await _saleDetailRepository.GetByProductIdAsync(productId);

            // ⚠️ CONSULTA HISTÓRICA: No aplicar validaciones de negocio
            // Muestra ventas históricas del producto
            
            return details ?? Enumerable.Empty<SaleDetail>();
        }
    }
}
