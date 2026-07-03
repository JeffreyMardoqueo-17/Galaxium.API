# 🧾 Reglas de Negocio — Proceso de Venta

## 1. Disponibilidad de productos

* Solo se pueden vender productos **activos**.
* Solo se pueden vender productos con **precio de venta asignado**.
* Solo se pueden vender productos con **stock mayor a 0**.

---

## 2. Validación de stock

* No se puede vender una cantidad mayor al stock disponible.
* Si el cliente solicita más unidades de las disponibles → la venta debe bloquearse.
* Esta validación debe existir en:

  * Frontend (UX)
  * Backend (regla obligatoria)

---

## 3. Venta multi-producto

* Una venta puede incluir **uno o varios productos**.
* Todos los productos seleccionados se registran en **una sola venta**.
* Cada producto genera un registro en **SaleDetail**.

---

## 4. Precio histórico

* El precio del producto se guarda al momento de la venta.
* Cambios futuros en el precio del producto **no afectan ventas pasadas**.

---

## 5. Cálculo de subtotales

* Subtotal por producto = `Quantity * UnitPrice`.
* El subtotal debe almacenarse (calculado o persistido).

---

## 6. Cálculo de total de venta

* Total = Suma de subtotales − Descuento.
* El backend es el único responsable de calcular totales.

---

## 7. Descuentos

* Se permite descuento general por venta.
* El descuento no puede ser negativo.
* El descuento no puede ser mayor al subtotal.

---

## 8. Cliente

* El cliente es opcional.
* Si se registra:

  * Se asocia a la venta.
  * Permite envío de factura.
  * Permite trazabilidad de compras.

---

## 9. Método de pago

* Toda venta debe tener método de pago.
* Ejemplos:

  * Efectivo
  * Transferencia
  * Tarjeta
  * Crédito

---

## 10. Descuento de inventario

* Al confirmarse la venta:

  * Se descuenta el stock vendido.
* Debe ejecutarse dentro de una **transacción**.

---

## 11. Inactivación automática

* Si el stock llega a 0:

  * El producto se marca como inactivo.

---

## 12. Integridad transaccional

La operación completa debe ser atómica:

Incluye:

* Crear venta
* Crear detalles
* Descontar stock
* Actualizar producto

Si algo falla → rollback total.

---

## 13. Concurrencia

* El stock debe revalidarse al momento de guardar.
* Evita ventas simultáneas que generen stock negativo.

---

## 14. Restricciones adicionales

* No se permiten ventas sin productos.
* No se permiten cantidades ≤ 0.
* No se permiten productos duplicados en el mismo detalle (deben consolidarse).

---

## 15. Auditoría básica

Cada venta debe registrar:

* Fecha
* Usuario vendedor
* Cliente (opcional)
* Método de pago
* Totales
* Detalles

---

#  Resultado esperado

Un proceso de venta debe garantizar:

* Exactitud financiera
* Integridad de inventario
* Historial de precios
* Trazabilidad de cliente
* Seguridad transaccional

### FLUJO 


Historial de ventas de un producto
```mermaid
flowchart TD

    A([🛒 Inicio de Venta])

    A --> B[Seleccionar Productos]

    %% ===========================
    %% VALIDACIONES
    %% ===========================

    B --> C{Validaciones}

    C --> C1[Producto Activo]
    C --> C2[Precio de Venta Asignado]
    C --> C3[Stock Mayor a 0]
    C --> C4[Cantidad Solicitada ≤ Stock]
    C --> C5[Sin Productos Duplicados]
    C --> C6[Cantidad Mayor que 0]

    C1 --> D
    C2 --> D
    C3 --> D
    C4 --> D
    C5 --> D
    C6 --> D

    %% ===========================
    %% DATOS DE LA VENTA
    %% ===========================

    D[Construcción de la Venta]

    D --> D1[Agregar uno o varios productos]
    D --> D2[Cliente Opcional]
    D --> D3[Método de Pago Obligatorio]
    D --> D4[Aplicar Descuento]

    D1 --> E
    D2 --> E
    D3 --> E
    D4 --> E

    %% ===========================
    %% CÁLCULOS
    %% ===========================

    E[Cálculos]

    E --> E1[Guardar Precio Histórico]
    E --> E2[Subtotal = Cantidad × Precio]
    E --> E3[Total = Σ Subtotales - Descuento]
    E --> E4[Backend Calcula Todo]

    E1 --> F
    E2 --> F
    E3 --> F
    E4 --> F

    %% ===========================
    %% TRANSACCIÓN
    %% ===========================

    F[Iniciar Transacción]

    F --> G[Crear Sale]

    G --> H[Crear SaleDetail]

    H --> I[Revalidar Stock]

    I --> J[Descontar Inventario]

    J --> K{¿Stock = 0?}

    K -- Sí --> L[Marcar Producto Inactivo]

    K -- No --> M[Mantener Producto Activo]

    L --> N

    M --> N

    N[Confirmar Transacción]

    %% ===========================
    %% ERROR
    %% ===========================

    I -->|Error| X[Rollback]

    G -->|Error| X

    H -->|Error| X

    J -->|Error| X

    X --> Z([Fin con Error])

    %% ===========================
    %% ÉXITO
    %% ===========================

    N --> O[Registrar Auditoría]

    O --> O1[Fecha]
    O --> O2[Usuario]
    O --> O3[Cliente]
    O --> O4[Método de Pago]
    O --> O5[Totales]
    O --> O6[Detalles]

    O6 --> P([Venta Exitosa])

    %% ===========================
    %% REPOSITORIOS
    %% ===========================

    P --> R1

    subgraph Repositorios

        R1[ISaleRepository]

        R1 --> R11[Crear Venta]
        R1 --> R12[Obtener por Id]
        R1 --> R13[Listar Ventas]
        R1 --> R14[Ventas por Fecha]
        R1 --> R15[Ventas por Cliente]

        R2[ISaleDetailRepository]

        R2 --> R21[Productos Vendidos]
        R2 --> R22[Top Productos]
        R2 --> R23[Detalles por Producto]
        R2 --> R24[Márgenes]
        R2 --> R25[Historial del Producto]

    end

    P --> R2

    %% ===========================
    %% ESTILOS
    %% ===========================

    classDef inicio fill:#0F766E,color:#fff,stroke:#0F766E;
    classDef validacion fill:#F59E0B,color:#fff,stroke:#D97706;
    classDef datos fill:#3B82F6,color:#fff,stroke:#2563EB;
    classDef calculos fill:#8B5CF6,color:#fff,stroke:#7C3AED;
    classDef transaccion fill:#10B981,color:#fff,stroke:#059669;
    classDef auditoria fill:#6366F1,color:#fff,stroke:#4338CA;
    classDef error fill:#DC2626,color:#fff,stroke:#991B1B;
    classDef repo fill:#374151,color:#fff,stroke:#111827;
    classDef fin fill:#16A34A,color:#fff,stroke:#15803D;

    class A inicio;

    class B,C,C1,C2,C3,C4,C5,C6 validacion;

    class D,D1,D2,D3,D4 datos;

    class E,E1,E2,E3,E4 calculos;

    class F,G,H,I,J,K,L,M,N transaccion;

    class O,O1,O2,O3,O4,O5,O6 auditoria;

    class X,Z error;

    class R1,R2,R11,R12,R13,R14,R15,R21,R22,R23,R24,R25 repo;

    class P fin;
```


ISaleRepository

Operaciones de negocio:

Crear venta completa

Obtener venta por Id

Listar ventas

Ventas por fecha

Ventas por cliente

ISaleDetailRepository

Operaciones analíticas / específicas:

Productos vendidos por fecha

Top productos

Detalles por producto

Márgenes