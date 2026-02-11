using Galaxium.Api.Enums;
using Galaxium.API.Entities;

namespace Galaxium.Api.Entities
{
    public class StockEntry
    {
        public int Id { get; set; }

        // ==========================
        // RELACIONES
        // ==========================
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // ==========================
        // INVENTARIO
        // ==========================
        public int Quantity { get; set; }

        public int RemainingQuantity { get; set; }

        //  YA NO ES NULLABLE
        public decimal UnitCost { get; set; }

        //  COMPUTED COLUMN (BD)
        public decimal TotalCost { get; private set; }

        // ==========================
        // REFERENCIA
        // ==========================
        public StockReferenceType ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        // ==========================
        // AUDITORÍA
        // ==========================
        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }
    }
}
