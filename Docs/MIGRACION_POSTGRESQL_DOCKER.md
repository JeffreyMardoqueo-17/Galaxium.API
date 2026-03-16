
# Documentación de la Migración y Deploy de Galaxium API a PostgreSQL + Docker Compose

## Objetivo
Migré la base de datos de SQL Server a PostgreSQL y levanté toda la infraestructura de la API con Docker Compose, manteniendo EF Core como ORM.

---

## Problemas que enfrenté y cómo los resolví

### 1. Migración de SQL Server a PostgreSQL
- **Problema:** El proyecto usaba Microsoft.EntityFrameworkCore.SqlServer y dependía de SQL Server.
- **Solución:**
  - Desinstalé el paquete de SQL Server.
  - Instalé `Npgsql.EntityFrameworkCore.PostgreSQL` y `Npgsql`.
  - Reemplacé `UseSqlServer(...)` por `UseNpgsql(...)` en `Program.cs`.

### 2. Configuración de la cadena de conexión
- **Problema:** La cadena de conexión estaba en los archivos de configuración y no en variables de entorno.
- **Solución:**
  - Eliminé la cadena de conexión de los archivos JSON.
  - Usé solo variables de entorno (`ConnectionStrings__DefaultConnection`) en Docker Compose y `.env`.

### 3. Migraciones vacías y errores de tabla
- **Problema:** La migración inicial estaba vacía, por lo que no se creaban tablas y la API fallaba con errores como `relation "Role" does not exist`.
- **Solución:**
  - Eliminé la migración vacía.
  - Generé una nueva migración con `dotnet ef migrations add InitialCreate`.
  - Verifiqué que la migración incluyera la creación de todas las tablas.

### 4. Seed de datos y errores de navegación
- **Problema:** El seed de roles devolvía `roleName: null` porque la propiedad de navegación no estaba cargada.
- **Solución:**
  - Ajusté el repositorio para recargar el usuario con `.Include(u => u.Role)` después de crearlo.
  - El mapeo de AutoMapper ya estaba preparado para devolver el nombre del rol si la propiedad estaba cargada.

### 5. Docker Compose y variables de entorno
- **Problema:** Inconsistencias entre `.env` y `compose.yaml` podían causar errores de autenticación.
- **Solución:**
  - Verifiqué que los valores de usuario, contraseña y base de datos fueran idénticos en ambos archivos.
  - Usé volúmenes y healthchecks para asegurar el arranque ordenado de los servicios.

### 6. Flujo de migración y despliegue
- **Problema:** La API intentaba hacer seed antes de que las migraciones se aplicaran correctamente.
- **Solución:**
  - Llamé a `db.Database.Migrate()` antes del seed en `Program.cs`.
  - Probé todo el flujo con `docker compose down -v` y `docker compose up --build -d`.

---

## Mi flujo de trabajo

1. **Preparar paquetes NuGet**
   - Desinstalé SQL Server, instalé Npgsql.
2. **Actualizar Program.cs**
   - Cambié a `UseNpgsql`, eliminé lógica de SQL Server, usé variables de entorno.
3. **Limpiar y regenerar migraciones**
   - Eliminé migraciones viejas, creé una nueva y la verifiqué.
4. **Revisar entidades y tipos especiales**
   - Confirmé tipos y propiedades especiales.
5. **Ajustar connection string**
   - Solo por variable de entorno.
6. **Configurar Dockerfile y Compose**
   - Usé imágenes oficiales, variables de entorno y healthchecks.
7. **Aplicar migraciones al arrancar**
   - Llamé a `Database.Migrate()` en el arranque.
8. **Validar en local con Docker**
   - Probé endpoints y seed.
9. **Documentar todo**
   - Este archivo.

---

## Notas finales
- El frontend Next.js no fue parte de esta tarea.
- En producción, las credenciales deben venir de un secrets manager.
- El seed de roles ahora siempre devuelve el nombre del rol correctamente.

---

**Fecha:** 2026-03-16
