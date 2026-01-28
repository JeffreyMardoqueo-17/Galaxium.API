using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.Entities
{
    public class StockMovement
    {
        public int Id { get; set; }

        // Producto afectado por el movimiento
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Usuario que realizó la acción
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Tipo de movimiento: IN / OUT
        public string MovementType { get; set; } = null!;

        // Cantidad movida
        public int Quantity { get; set; }

        // Referencia del movimiento: SALE, ADJUSTMENT, PURCHASE
        public string? Reference { get; set; }

        // Fecha del movimiento}
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}