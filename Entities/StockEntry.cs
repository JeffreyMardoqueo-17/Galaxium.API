using Galaxium.Api.Enums;
using Galaxium.API.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Galaxium.Api.Entities
{
    public class StockEntry
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int RemainingQuantity { get; set; }


        // + entra stock | - sale stock
        public int Quantity { get; set; }

        // Solo se usa cuando ReferenceType = PURCHASE
        public decimal? UnitCost { get; set; }

        // PURCHASE | SALE | ADJUSTMENT
        public StockReferenceType ReferenceType { get; set; }


        // Id de la venta, ajuste, etc.
        public int? ReferenceId { get; set; }

        public DateTime CreatedAt { get; set; }

        // 🔥 AQUÍ VA (NO SE GUARDA EN BD)
        public decimal TotalCost =>
            UnitCost.HasValue ? UnitCost.Value * Quantity : 0;
    }
}
