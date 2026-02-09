using System;
using System.Collections.Generic;
using Galaxium.Api.Entities;

namespace Galaxium.API.Entities
{
    public class Sale
    {
        public int Id { get; set; }

        /* ============================
           FOREIGN KEYS
        ============================ */
        public int? CustomerId { get; set; }
        public int UserId { get; set; }
        public int PaymentMethodId { get; set; }

        /* ============================
           FINANCIAL DATA
        ============================ */
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal AmountPaid { get; set; } //dinero recivido --solo para efectivo
        public decimal ChangeAmount { get; set; } //vuelto entregado --solo para efectivo
        public decimal Total { get; set; }

        /* ============================
           SALE INFO
        ============================ */
        public DateTime SaleDate { get; set; }
        public string Status { get; set; } = "COMPLETED";
        public string? InvoiceNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        /* ============================
           NAVIGATION PROPERTIES
        ============================ */
        public Customer? Customer { get; set; }
        public User User { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; } = null!;

        public ICollection<SaleDetail> Details { get; set; }
            = new List<SaleDetail>();
    }
}
