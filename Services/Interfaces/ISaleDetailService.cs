using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.Interfaces
{
    public interface ISaleDetailService
    {
        // Obtener detalles por venta
        Task<IEnumerable<SaleDetail>> GetDetailsBySaleIdAsync(int saleId);

        // Obtener detalles por producto
        Task<IEnumerable<SaleDetail>> GetDetailsByProductIdAsync(int productId);
    }
}