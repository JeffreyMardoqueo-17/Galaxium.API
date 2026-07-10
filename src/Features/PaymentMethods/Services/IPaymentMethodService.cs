using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Entities;

namespace Galaxium.API.Services.Interfaces
{
    public interface IPaymentMethodService
    {
        
        Task<IEnumerable<PaymentMethod>> GetAllAsync();
        Task<PaymentMethod?> GetByIdAsync(int id);
    }
}