using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.Entities
{
    public class Customer
    {
                public int Id { get; set; }

        // Nombre completo del cliente
        public string FullName { get; set; } = null!;

        // Teléfono de contacto
        public string? Phone { get; set; }

        // Correo electrónico
        public string? Email { get; set; }

        // Fecha de registro
        public DateTime CreatedAt { get; set; }

        // Navegación: ventas realizadas por el cliente
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }

}