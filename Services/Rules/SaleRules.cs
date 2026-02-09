using System;

namespace Galaxium.Api.Services.Rules
{
    public class SaleRules
    {
        // ==========================================
        // 1. Validar que la venta tenga productos
        // ==========================================
        public void ValidateHasProducts(int totalItems)
        {
            if (totalItems <= 0)
                throw new InvalidOperationException(
                    "No se puede registrar una venta sin productos.");
        }

        // ==========================================
        // 2. Validar método de pago
        // ==========================================
        public void ValidatePaymentMethod(int paymentMethodId)
        {
            if (paymentMethodId <= 0)
                throw new ArgumentException(
                    "Debe seleccionar un método de pago válido.");
        }

        // ==========================================
        // 3. Validar vendedor
        // ==========================================
        public void ValidateSeller(int sellerUserId)
        {
            if (sellerUserId <= 0)
                throw new ArgumentException(
                    "La venta debe tener un vendedor válido.");
        }

        // ==========================================
        // 4. Validar subtotal
        // ==========================================
        public void ValidateSubTotal(decimal subTotal)
        {
            if (subTotal <= 0)
                throw new InvalidOperationException(
                    "El subtotal de la venta debe ser mayor a cero.");
        }

        // ==========================================
        // 5. Validar descuento
        // ==========================================
        public void ValidateDiscount(decimal discount)
        {
            if (discount < 0)
                throw new InvalidOperationException(
                    "El descuento no puede ser negativo.");
        }

        // ==========================================
        // 6. Descuento no puede exceder subtotal
        // ==========================================
        public void ValidateDiscountLimit(decimal discount, decimal subTotal)
        {
            if (discount > subTotal)
                throw new InvalidOperationException(
                    "El descuento no puede ser mayor al subtotal.");
        }

        // ==========================================
        // 7. Calcular total
        // ==========================================
        public decimal CalculateTotal(decimal subTotal, decimal discount)
        {
            return subTotal - discount;
        }

        // ==========================================
        // 8. Validar total final
        // ==========================================
        public void ValidateTotal(decimal total)
        {
            if (total < 0)
                throw new InvalidOperationException(
                    "El total de la venta no puede ser negativo.");
        }

        // ==========================================
        // 9. Generar número de factura
        // ==========================================
        public string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        // ==========================================
        // 10. Validar estado de venta
        // ==========================================
        public void ValidateStatus(string status)
        {
            var validStatuses = new[] { "COMPLETED", "CANCELLED", "REFUNDED" };

            if (!Array.Exists(validStatuses, s => s == status))
                throw new InvalidOperationException(
                    "Estado de venta inválido.");
        }

        // ==========================================
        // 11. Validar monto pagado (para efectivo)
        // ==========================================
        public void ValidateAmountPaid(decimal? amountPaid, decimal total, int paymentMethodId)
        {
            // Si el método de pago NO es efectivo (asumimos que efectivo es ID = 1), no validar
            // Ajusta este ID según tu base de datos
            if (paymentMethodId != 1)
                return;

            // Si es efectivo, debe haber un monto pagado
            if (!amountPaid.HasValue || amountPaid.Value <= 0)
                throw new InvalidOperationException(
                    "Para pagos en efectivo debe especificar el monto recibido.");

            // El monto pagado debe ser mayor o igual al total
            if (amountPaid.Value < total)
                throw new InvalidOperationException(
                    $"El monto pagado ({amountPaid:C}) es insuficiente. El total es {total:C}.");
        }

        // ==========================================
        // 12. Calcular vuelto
        // ==========================================
        public decimal CalculateChange(decimal amountPaid, decimal total)
        {
            var change = amountPaid - total;
            return change < 0 ? 0 : change;
        }
    }
}
