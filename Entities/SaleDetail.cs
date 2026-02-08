using System;

namespace Galaxium.API.Entities
{
    public class SaleDetail
    {
        public int Id { get; set; }

        /* ============================
           FOREIGN KEYS
        ============================ */
        public int SaleId { get; set; }
        public int ProductId { get; set; }

        /* ============================
           PRODUCT DATA
        ============================ */
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }

        // Columna calculada en SQL
        public decimal SubTotal { get; private set; }

        public DateTime CreatedAt { get; set; }

        /* ============================
           NAVIGATION
        ============================ */
        public Sale Sale { get; set; } = null!;
        public Product Product { get; set; } = null!;

        /* ============================
           MÉTODO PARA CALCULAR SUBTOTAL
        ============================ */
        public void CalculateSubTotal()
        {
            SubTotal = Quantity * UnitPrice;
        }
    }
}
