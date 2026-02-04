# 📦 Gestión de Inventario – Galaxium POS

Este documento describe el **modelo de negocio y flujo operativo** para el manejo de productos, stock y ventas en el sistema Galaxium.

El **inventario es la vida del sistema**, por lo tanto:
- Nunca se manipula el stock “a mano”
- Todo movimiento debe quedar auditado
- El producto y el stock **no son lo mismo**

---

## 🔄 Entidades Clave para Gestionar el Stock

### 🧾 Producto (`Product`)

Tabla que define la **identidad del producto**.

Contiene:
- Nombre
- Categoría
- SKU
- Precio de costo
- Precio de venta
- Estado (activo/inactivo)
- Stock total (campo derivado)

> ⚠️ **Regla crítica**  
> El campo `Stock` **NO se modifica directamente**.  
> Es un **resumen** calculado a partir de las entradas y salidas reales.

Responsabilidad:
- Definir qué es el producto
- Controlar si se puede vender (`IsActive`)
- Mostrar stock disponible

---

### 📥 Entrada de Stock (`StockEntry`)

Registra **cada ingreso físico de inventario** (por lote).

Contiene:
- Producto
- Usuario que registra
- Cantidad ingresada
- Costo unitario
- Costo total (calculado)
- Fecha
- Estado del lote (`IsActive`)

> ✅ Regla de negocio  
> **Cada ingreso de stock = un nuevo registro en `StockEntry`**

Esto permite:
- Saber cuánto se invirtió
- Saber de qué lote salió cada producto
- Auditoría real de inventario

Ejemplos:
- Compra a proveedor
- Ajuste positivo
- Corrección manual autorizada

---

### 🔁 Movimiento de Stock (`StockMovement`)

Historial **completo e inmutable** de movimientos.

Registra:
- Producto
- Usuario
- Tipo de movimiento: `IN` / `OUT`
- Cantidad
- Referencia (`SALE`, `PURCHASE`, `ADJUSTMENT`)
- Fecha

> 🧠 Regla de negocio  
> **Nada entra ni sale del inventario sin dejar rastro aquí**

Usos:
- Auditoría
- Reportes
- Trazabilidad
- Seguridad

---

### 🧾 Detalle de Venta (`SaleDetail`)

Registra los productos vendidos.

Aunque no es inventario directo:
- Cada venta genera una **salida de stock**
- El trigger de base de datos:
  - Descuenta el stock del producto
  - Registra el movimiento `OUT` automáticamente

> ✅ La venta **nunca toca el stock directamente**, solo dispara el flujo correcto

---

## 🔄 Flujo Completo de Manejo de Inventario

### 1️⃣ Creación del Producto

- Se registra el producto (identidad)
- Stock inicial = `0`
- No se puede vender hasta que tenga stock

## 🧱 Principio Base

**Producto creado ≠ Producto disponible**

Un producto puede existir en el sistema sin estar disponible para la venta.  
La disponibilidad depende exclusivamente del **stock** y del **estado del producto**.

---

## 2️⃣ Registro de Entrada de Stock

### Flujo

1. El usuario selecciona el producto.
2. Ingresa la cantidad de unidades.
3. Ingresa el costo unitario.
4. Se crea un registro en `StockEntry`.
5. Se registra un movimiento en `StockMovement` con tipo **IN**.
6. Se actualiza el stock total del producto.

### Regla clave

📌 **Si ya existe stock anterior, no se mezcla.**  
Cada entrada representa un **lote independiente**, con su propio costo y fecha.

---

## 3️⃣ Registro de Salida de Stock (Venta)

### Flujo

1. Se crea la venta (`Sale`).
2. Se insertan los registros en `SaleDetail`.
3. Se ejecuta el trigger automático:
   - Descuenta el stock del producto.
   - Registra un movimiento en `StockMovement` con tipo **OUT**.

### Validaciones obligatorias

- El producto debe tener **stock suficiente**.
- El producto debe estar **activo**.

### Reglas duras

❌ Si el stock es `0` → no se puede vender  
❌ Si el producto está inactivo → no se muestra en la pantalla de ventas

---

## 4️⃣ Alertas de Stock

### Reglas operativas

- Si `Stock <= MinimumStock` → alerta visual de bajo inventario
- Si `Stock == 0` → producto agotado
- Si `IsActive == false` → producto oculto en ventas

> Estas alertas son **información operativa**, no validaciones duras de negocio.

---

## 5️⃣ Ajustes y Auditoría

Para cualquier corrección de inventario:

- Se registra un movimiento en `StockMovement`
- El tipo de movimiento es **ADJUSTMENT**
- **Nunca** se edita el stock directamente en la tabla `Product`

### Ejemplos de ajustes

- Pérdida de producto
- Producto dañado
- Diferencias en conteo físico

---

## 🧠 Principios Clave del Sistema

- El producto **no es** el stock
- El stock es un **resultado**, no una entrada directa
- Todo movimiento debe ser **trazable**
- Sin historial → el sistema está roto
- Sin reglas → hay pérdidas

---

## 📌 Conclusión

Este modelo permite:

- Escalar el sistema sin perder control
- Reducir y detectar pérdidas
- Saber cuánto dinero hay invertido en inventario
- Saber quién hizo qué y cuándo
- Mantener la **integridad total del inventario**

👉 **Este flujo es la columna vertebral del POS.**
