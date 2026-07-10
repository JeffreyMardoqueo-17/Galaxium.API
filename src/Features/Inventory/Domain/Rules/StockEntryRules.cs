using System;

namespace Galaxium.Api.Services.Rules
{
    public class StockEntryRules
    {
        // ===============================
        // 1. Cantidad debe ser mayor a 0
        // ===============================
        public void ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException(
                    "La cantidad de stock debe ser mayor a cero.");
        }

        // ===============================
        // 2. Costo unitario válido
        // ===============================
        public void ValidateUnitCost(decimal unitCost)
        {
            if (unitCost <= 0)
                throw new ArgumentException(
                    "El costo unitario debe ser mayor a cero.");
        }

        // ===============================
        // 3. Producto obligatorio
        // ===============================
        public void ValidateProductExists(object? product)
        {
            if (product == null)
                throw new ArgumentException(
                    "El producto no existe.");
        }

        // ===============================
        // 4. Usuario obligatorio
        // ===============================
        public void ValidateUser(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException(
                    "Usuario inválido para registrar stock.");
        }

        // ===============================
        // 5. Evitar duplicados sospechosos
        // ===============================
        public void ValidateDuplicateEntry(
            int quantity,
            decimal unitCost,
            DateTime lastEntryDate)
        {
            var minutes = (DateTime.Now - lastEntryDate).TotalMinutes;

            if (minutes < 2)
                throw new InvalidOperationException(
                    "Movimiento sospechoso: ya registraste un lote similar hace unos minutos.");
        }

        // ===============================
        // 6. Límite de cantidad extrema
        // ===============================
        public void ValidateExtremeQuantity(int quantity)
        {
            if (quantity > 10000)
                throw new InvalidOperationException(
                    "Cantidad demasiado alta. Requiere autorización administrativa.");
        }

        // ===============================
        // 7. Validar coherencia costo vs producto
        // ===============================
        public void ValidateCostVariation(
            decimal newCost,
            decimal? lastCost)
        {
            if (lastCost == null) return;

            var variation = Math.Abs(newCost - lastCost.Value) / lastCost.Value;

            if (variation > 0.50m) // >50%
                throw new InvalidOperationException(
                    "El costo varía demasiado respecto a la última compra. Verifica el dato.");
        }

        // ===============================
        // 8. Inicializar RemainingQuantity
        // ===============================
        public int InitializeRemaining(int quantity)
        {
            return quantity;
        }
    }
}
