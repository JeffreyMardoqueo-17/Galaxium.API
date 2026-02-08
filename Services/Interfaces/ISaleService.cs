using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.Interfaces
{
   public interface ISaleService
    {
        // Crear venta completa (cabecera + detalles + reglas de negocio)
        Task<Sale> CreateSaleAsync(
            Sale sale,
            IEnumerable<SaleDetail> saleDetails
        );

        // Consultas
        Task<Sale?> GetSaleByIdAsync(int saleId);

        Task<IEnumerable<Sale>> GetAllSalesAsync();

        Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime start, DateTime end);

        Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId);
    }
}