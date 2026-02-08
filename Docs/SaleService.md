# Documentación del Servicio de Ventas (`SaleService`)

## 1. Funcionalidad general

El `SaleService` se encarga de la creación y gestión de ventas completas, incluyendo cabecera (`Sale`) y detalles (`SaleDetail`). Integra reglas de negocio y validaciones antes de persistir los datos en la base.

---

## 2. Creación de venta completa (`CreateSaleAsync`)

### Flujo de ejecución

1. **Validaciones generales de la venta**
   - Verifica que existan productos.
   - Valida que el vendedor (`SellerUserId`) sea válido.
   - Valida que el método de pago (`PaymentMethodId`) sea correcto.

2. **Validación de cada detalle de producto**
   - Producto activo.
   - Precio de venta asignado.
   - Cantidad mayor a cero.
   - Stock suficiente.

3. **Cálculo de subtotales y totales**
   - Asigna `UnitPrice` y `UnitCost`.
   - Calcula `SubTotal` por detalle usando `SaleDetail.CalculateSubTotal()`.
   - Suma todos los `SubTotal` para obtener `Sale.SubTotal`.
   - Valida y aplica descuentos.
   - Calcula `Total` final y valida.

4. **Asignaciones adicionales**
   - Genera número de factura (`InvoiceNumber`).
   - Asigna fecha de venta (`SaleDate`).
   - Asigna estado inicial (`COMPLETED`).

5. **Persistencia**
   - Llama a `SaleRepository.CreateSaleWithDetailsAsync` para guardar la venta completa y actualizar stock.
   - La operación se realiza dentro de una transacción para garantizar consistencia.

---

## 3. Consultas de ventas

### `GetSaleByIdAsync`
- Valida que el ID sea mayor a cero.
- Retorna la venta con todos los detalles y relaciones (`Customer`, `SellerUser`, `PaymentMethod`, `Product`).
- Lanza excepción si no existe.

### `GetAllSalesAsync`
- Retorna todas las ventas.
- Lanza excepción si no hay ventas.

### `GetSalesByDateRangeAsync`
- Valida que la fecha de inicio no sea mayor a la fecha final.
- Limita el rango máximo a 10 años.
- Retorna ventas en el rango.
- Lanza excepción si no hay ventas.

### `GetSalesByCustomerAsync`
- Valida que el ID del cliente sea mayor a cero.
- Retorna ventas del cliente.
- Lanza excepción si no hay ventas.

---

## 4. Reglas de negocio aplicadas

### Reglas de la venta (`SaleRules`)
- Validar existencia de productos.
- Validar vendedor y método de pago.
- Validar subtotal, descuento y total.
- Generar número de factura.
- Validar estado de venta.

### Reglas de detalle (`SaleDetailsRules`)
- Validar producto activo.
- Validar precio unitario y costo.
- Validar stock disponible y cantidad solicitada.
- Calcular subtotal por producto.
- Validar duplicidad de productos.
- Actualizar stock y determinar inactivación automática si stock = 0.

---

## 5. Consideraciones importantes

- `SaleDetail.SubTotal` debe tener un método para calcularlo:

```csharp
public void CalculateSubTotal()
{
    SubTotal = Quantity * UnitPrice;
}
```
