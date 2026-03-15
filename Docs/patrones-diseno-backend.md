# Patrones de diseno aplicados en el backend de Galaxium

En esta fase del backend aplique patrones de diseno para mejorar la mantenibilidad, separar responsabilidades y reducir el acoplamiento entre servicios y repositorios.

## Objetivo

Mi objetivo fue dejar una estructura mas limpia para operaciones criticas de negocio, especialmente en ventas e inventario, donde necesito consistencia transaccional y reglas por tipo de movimiento.

## 1. Unit of Work

### Que implemente

Implemente un Unit of Work para centralizar el manejo de transacciones y guardado de cambios.

- Contrato: [Repository/Interfaces/IUnitOfWork.cs](../Repository/Interfaces/IUnitOfWork.cs)
- Implementacion: [Repository/repos/UnitOfWork.cs](../Repository/repos/UnitOfWork.cs)

### Donde lo use

- En el flujo de ventas: [Services/service/SaleService.cs](../Services/service/SaleService.cs)
- En el flujo de entradas de stock: [Services/service/StockEntryService.cs](../Services/service/StockEntryService.cs)

### Por que lo use

Lo use para garantizar que una operacion de negocio se complete de forma atomica: o se guarda todo, o no se guarda nada. Con esto evite duplicar logica transaccional dentro de cada repositorio.

### Ajuste arquitectonico que hice

Removi transacciones internas en repositorios para dejar una sola frontera transaccional en la capa de servicio con Unit of Work.

- [Repository/repos/SaleRepository.cs](../Repository/repos/SaleRepository.cs)
- [Repository/repos/StockEntryRepository.cs](../Repository/repos/StockEntryRepository.cs)

## 2. Factory Method (movimientos de inventario)

### Que implemente

Convierti la logica por tipo de movimiento de stock en handlers especializados y una fabrica para resolver el handler adecuado segun el tipo.

- Contrato de handler: [Services/Interfaces/IStockMovementHandler.cs](../Services/Interfaces/IStockMovementHandler.cs)
- Contrato de factory: [Services/Interfaces/IStockMovementHandlerFactory.cs](../Services/Interfaces/IStockMovementHandlerFactory.cs)
- Factory concreta: [Services/service/StockMovements/StockMovementHandlerFactory.cs](../Services/service/StockMovements/StockMovementHandlerFactory.cs)

Handlers por tipo:

- Compra: [Services/service/StockMovements/PurchaseStockMovementHandler.cs](../Services/service/StockMovements/PurchaseStockMovementHandler.cs)
- Venta: [Services/service/StockMovements/SaleStockMovementHandler.cs](../Services/service/StockMovements/SaleStockMovementHandler.cs)
- Ajuste: [Services/service/StockMovements/AdjustmentStockMovementHandler.cs](../Services/service/StockMovements/AdjustmentStockMovementHandler.cs)

### Donde lo use

- [Services/service/StockEntryService.cs](../Services/service/StockEntryService.cs)

### Por que lo use

Lo use para eliminar condicionales grandes y dejar el comportamiento por tipo encapsulado. Asi, cuando agregue un nuevo tipo de movimiento, solo tendre que crear un nuevo handler sin tocar la logica existente.

## 3. Singleton

### Donde ya lo tenia aplicado

Mantengo el cliente de Cloudinary como Singleton en inyeccion de dependencias.

- Registro en DI: [Program.cs](../Program.cs)

### Por que lo use

Lo use porque es una dependencia de infraestructura compartida y no tiene sentido crear una nueva instancia en cada request.

## Registro de dependencias (DI)

Registre Unit of Work, handlers y factory en el contenedor de dependencias para que el backend resuelva cada componente automaticamente.

- [Program.cs](../Program.cs)

## Beneficios que obtuve

1. Consistencia transaccional en operaciones criticas.
2. Menor acoplamiento entre servicio y detalles de infraestructura.
3. Codigo mas facil de extender y mantener.
4. Mejor separacion de responsabilidades entre reglas de negocio, persistencia y orquestacion.

## Cierre

Con estos cambios deje aplicado Unit of Work para la consistencia de negocio, Factory Method para la variabilidad de reglas en inventario y Singleton para infraestructura compartida. Esto me deja una base mas profesional y escalable para seguir evolucionando el backend.
