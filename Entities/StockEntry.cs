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

        public int Quantity { get; set; }
        public int RemainingQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; private set; }


        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

}
