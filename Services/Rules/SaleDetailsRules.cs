using System;

namespace Galaxium.Api.Services.Rules
{
    public class SaleDetailsRules
    {
        // ==========================================
        // 1. Validar producto activo
        // ==========================================
        public void ValidateProductIsActive(bool isActive)
        {
            if (!isActive)
                throw new InvalidOperationException(
                    "No se pueden vender productos inactivos.");
        }

        // ==========================================
        // 2. Validar precio de venta
        // ==========================================
        public void ValidateProductHasPrice(decimal? salePrice)
        {
            if (salePrice == null || salePrice <= 0)
                throw new InvalidOperationException(
                    "El producto no tiene precio de venta asignado.");
        }

        // ==========================================
        // 3. Validar stock disponible
        // ==========================================
        public void ValidateStockAvailable(int stock)
        {
            if (stock <= 0)
                throw new InvalidOperationException(
                    "El producto no tiene stock disponible.");
        }

        // ==========================================
        // 4. Validar cantidad solicitada
        // ==========================================
        public void ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException(
                    "La cantidad a vender debe ser mayor a cero.");
        }

        // ==========================================
        // 5. Validar que no exceda stock
        // ==========================================
        public void ValidateQuantityAgainstStock(
            int quantityRequested,
            int stockAvailable)
        {
            if (quantityRequested > stockAvailable)
                throw new InvalidOperationException(
                    $"Stock insuficiente. Disponible: {stockAvailable}");
        }

        // ==========================================
        // 6. Calcular subtotal por producto
        // ==========================================
        public decimal CalculateSubTotal(
            int quantity,
            decimal unitPrice)
        {
            return quantity * unitPrice;
        }

        // ==========================================
        // 7. Validar precio unitario
        // ==========================================
        public void ValidateUnitPrice(decimal price)
        {
            if (price <= 0)
                throw new InvalidOperationException(
                    "El precio unitario debe ser mayor a cero.");
        }

        // ==========================================
        // 8. Validar costo unitario
        // ==========================================
        public void ValidateUnitCost(decimal cost)
        {
            if (cost < 0)
                throw new InvalidOperationException(
                    "El costo unitario no puede ser negativo.");
        }

        // ==========================================
        // 9. Validar duplicidad de producto
        // ==========================================
        public void ValidateDuplicateProduct(
            bool alreadyExists)
        {
            if (alreadyExists)
                throw new InvalidOperationException(
                    "El producto ya fue agregado a la venta. Consolide cantidades.");
        }

        // ==========================================
        // 10. Calcular nuevo stock restante
        // ==========================================
        public int CalculateRemainingStock(
            int currentStock,
            int quantitySold)
        {
            return currentStock - quantitySold;
        }

        // ==========================================
        // 11. Validar stock negativo post-venta
        // ==========================================
        public void ValidateRemainingStock(int remaining)
        {
            if (remaining < 0)
                throw new InvalidOperationException(
                    "La operación genera stock negativo.");
        }

        // ==========================================
        // 12. Determinar inactivación automática
        // ==========================================
        public bool ShouldDeactivateProduct(int remainingStock)
        {
            return remainingStock == 0;
        }
    }
}
