using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Repository.Interfaces;
using Galaxium.API.Services.Interfaces;
using Galaxium.API.Common;
using Galaxium.Api.Entities;

namespace Galaxium.API.Services.service
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IPaymentMethodRepository _repositoy;
        public PaymentMethodService(IPaymentMethodRepository repository)
        {
            _repositoy = repository;
        }

        public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
        {
            var metodos = await _repositoy.GetAllAsync();
            if (metodos == null || !metodos.Any())
            {
            throw new NotFoundBusinessException("No se encontraron métodos de pago activos.");
            }
            return metodos;
        }
        public async Task<PaymentMethod?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser un número positivo.", nameof(id));
            var metodo = await _repositoy.GetByIdAsync(id);
            if (metodo == null)
                throw new NotFoundBusinessException($"No se encontró el método de pago con ID {id}.");
            return metodo;
        }
    }
}