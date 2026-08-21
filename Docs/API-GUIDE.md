# Galaxium ERP API — Documentación Completa para Frontend

> **Versión:** v1 | **Base URL:** `http://localhost:5213` | **Swagger UI:** `http://localhost:5213/`

---

## Tabla de Contenidos

1. [Autenticación y Autorización](#1-autenticación-y-autorización)
2. [Flujo de Onboarding (Crear Empresa)](#2-flujo-de-onboarding-crear-empresa)
3. [Flujo de Login](#3-flujo-de-login)
4. [Flujo de Refresh Token](#4-flujo-de-refresh-token)
5. [Flujo de Logout](#5-flujo-de-logout)
6. [Catálogo Completo de Endpoints](#6-catálogo-completo-de-endpoints)
7. [Códigos de Respuesta HTTP](#7-códigos-de-respuesta-http)
8. [Guía de Integración Frontend](#8-guía-de-integración-frontend)

---

## 1. Autenticación y Autorización

### Esquema de Autenticación

La API utiliza **JWT Bearer Tokens** con **HttpOnly Cookies**.

```
Authorization: Bearer <access_token>
```

> **Nota:** El frontend NO necesita enviar el header `Authorization` manualmente si usa cookies. El navegador lo envía automáticamente con `credentials: 'include'`.

### Cookies de Autenticación

| Cookie | Tipo | Duración | Descripción |
|--------|------|----------|-------------|
| `access_token` | HttpOnly, Secure | 12 horas | Token de acceso JWT |
| `refresh_token` | HttpOnly, Secure | 30 días | Token de refresco para renovar el access token |

### Claims del JWT

El token de acceso contiene los siguientes claims:

| Claim | Tipo | Descripción |
|-------|------|-------------|
| `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` | string | ID del usuario |
| `tenant_id` | int | ID del tenant (empresa) |
| `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name` | string | Username del usuario |
| `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role` | string | Nombre del rol (ej: "Administrador") |
| `tenant_name` | string | Nombre del tenant |
| `jti` | GUID | Identificador único del token |
| `exp` | long | Fecha de expiración (Unix timestamp) |

### Políticas de Autorización

| Política | Roles Permitidos |
|----------|------------------|
| `AdminOnly` | Administrador |
| `AdminOrSupervisor` | Administrador, Supervisor |
| `InventoryManagement` | Administrador, Supervisor, Encargado de inventario |
| `SalesAccess` | Administrador, Supervisor, Cajero |
| `ReportsAccess` | Administrador, Supervisor |

---

## 2. Flujo de Onboarding (Crear Empresa)

### `POST /api/Tenant/onboard`

Crea una nueva empresa (**Tenant**) con su usuario **Administrador** y rol en una sola transacción. Si falla cualquier operación, se hace rollback automático.

**Autenticación:** No requiere (AllowAnonymous)

#### Request Body

```json
{
  "tenant": {
    "name": "Farmacia Central",
    "slug": "farmacia-central",
    "contactEmail": "contacto@farmaciacentral.com",
    "phoneNumber": "+50370002222",
    "address": "San Miguel, El Salvador",
    "maxUsers": 50,
    "maxProducts": 5000
  },
  "administrator": {
    "fullName": "Juan Pérez",
    "username": "admin",
    "email": "admin@farmaciacentral.com",
    "password": "Admin123*"
  }
}
```

#### Campos del Tenant

| Campo | Tipo | Requerido | Máx. Length | Descripción |
|-------|------|-----------|-------------|-------------|
| `name` | string | **Sí** | 150 | Nombre de la empresa |
| `slug` | string | No | 150 | Identificador URL-friendly (se guarda en minúsculas) |
| `contactEmail` | string | No | 150 | Correo de contacto de la empresa |
| `phoneNumber` | string | No | 30 | Teléfono de contacto |
| `address` | string | No | 300 | Dirección física |
| `maxUsers` | int | No | — | Máximo de usuarios permitidos (default: 50) |
| `maxProducts` | int | No | — | Máximo de productos permitidos (default: 1000) |

#### Campos del Administrador

| Campo | Tipo | Requerido | Máx. Length | Descripción |
|-------|------|-----------|-------------|-------------|
| `fullName` | string | **Sí** | 100 | Nombre completo del administrador |
| `username` | string | **Sí** | 50 | Username para login |
| `email` | string | **Sí** | 150 | Correo electrónico del administrador |
| `password` | string | **Sí** | — | Contraseña (mín. 8 caracteres) |

#### Validaciones de Contraseña

La contraseña debe cumplir TODAS las siguientes reglas:

- Mínimo 8 caracteres
- Al menos 1 letra mayúscula
- Al menos 1 letra minúscula
- Al menos 1 número
- Al menos 1 carácter especial (ej: `*`, `!`, `@`, `#`)

#### Response — 201 Created

```json
{
  "tenantId": 2,
  "tenantName": "Farmacia Central",
  "administratorUserId": 1,
  "administratorUsername": "admin",
  "message": "Tenant y administrador creados correctamente."
}
```

#### Response — 400 Bad Request (Validación)

```json
{
  "message": "El slug 'farmacia-central' ya está en uso."
}
```

```json
{
  "message": "La contraseña debe contener al menos un número."
}
```

#### Response — 409 Conflict (Duplicado)

```json
{
  "message": "El correo 'contacto@farmaciacentral.com' ya está registrado."
}
```

#### Ejemplo con cURL

```bash
curl -X POST http://localhost:5213/api/Tenant/onboard \
  -H "Content-Type: application/json" \
  -d '{
    "tenant": {
      "name": "Farmacia Central",
      "slug": "farmacia-central",
      "contactEmail": "contacto@farmaciacentral.com",
      "phoneNumber": "+50370002222",
      "address": "San Miguel",
      "maxUsers": 50,
      "maxProducts": 5000
    },
    "administrator": {
      "fullName": "Juan Pérez",
      "username": "admin",
      "email": "admin@farmaciacentral.com",
      "password": "Admin123*"
    }
  }'
```

#### Ejemplo con JavaScript (fetch)

```javascript
async function onboardCompany(data) {
  const response = await fetch('http://localhost:5213/api/Tenant/onboard', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result.message || 'Error en el onboarding');
  }

  return result;
}

// Uso
const result = await onboardCompany({
  tenant: {
    name: "Farmacia Central",
    slug: "farmacia-central",
    contactEmail: "contacto@farmaciacentral.com",
    phoneNumber: "+50370002222",
    address: "San Miguel",
    maxUsers: 50,
    maxProducts: 5000
  },
  administrator: {
    fullName: "Juan Pérez",
    username: "admin",
    email: "admin@farmaciacentral.com",
    password: "Admin123*"
  }
});

console.log(result.tenantId); // 2
console.log(result.message);  // "Tenant y administrador creados correctamente."
```

---

## 3. Flujo de Login

### `POST /api/User/login`

Autentica al usuario y devuelve cookies de acceso.

**Autenticación:** No requiere

#### Request Body

```json
{
  "username": "admin",
  "password": "Admin123*"
}
```

#### Response — 200 OK

```json
{
  "userId": 1,
  "fullName": "Juan Pérez",
  "username": "admin",
  "roleName": "Administrador",
  "tenantId": 2,
  "tenantName": "Farmacia Central"
}
```

> **Importante:** Las cookies `access_token` y `refresh_token` se establecen automáticamente en la respuesta como HttpOnly cookies. El frontend NO necesita manejar los tokens manualmente.

#### Ejemplo con JavaScript

```javascript
async function login(username, password) {
  const response = await fetch('http://localhost:5213/api/User/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include', // IMPORTANTE: para recibir las cookies
    body: JSON.stringify({ username, password })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Credenciales incorrectas');
  }

  return await response.json();
}

// Uso
const user = await login('admin', 'Admin123*');
console.log(user.roleName); // "Administrador"
```

---

## 4. Flujo de Refresh Token

### `POST /api/User/refresh`

Renueva el access token usando el refresh token (almacenado en cookie HttpOnly).

**Autenticación:** No requiere (usa cookies)

#### Request Body

```json
{
  "expiredAccessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

> **Nota:** Si el access token ya expiró, envíalo en `expiredAccessToken` para que el backend valide que pertenece al mismo usuario. Si aún no expiró, puedes omitirlo.

#### Response — 200 OK

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

#### Ejemplo con JavaScript

```javascript
async function refreshTokens() {
  const response = await fetch('http://localhost:5213/api/User/refresh', {
    method: 'POST',
    credentials: 'include'
  });

  if (!response.ok) {
    // Refresh token expirado o revocado → redirigir a login
    window.location.href = '/login';
    return;
  }

  return await response.json();
}
```

---

## 5. Flujo de Logout

### `POST /api/User/logout`

Cierra la sesión del usuario revocando el refresh token y eliminando las cookies.

**Autenticación:** Requiere (cualquier usuario autenticado)

#### Response — 200 OK

```json
{
  "message": "Sesión cerrada correctamente."
}
```

#### Ejemplo con JavaScript

```javascript
async function logout() {
  await fetch('http://localhost:5213/api/User/logout', {
    method: 'POST',
    credentials: 'include'
  });

  window.location.href = '/login';
}
```

---

## 6. Catálogo Completo de Endpoints

### Tenant (Empresas)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/Tenant/onboard` | No | Crear empresa + administrador |
| `POST` | `/api/Tenant` | No* | Crear primer tenant (solo si no existe ninguno) |
| `GET` | `/api/Tenant` | AdminOnly | Listar todas las empresas |
| `GET` | `/api/Tenant/{tenantId}` | AdminOnly | Obtener empresa por ID |
| `PUT` | `/api/Tenant/{tenantId}` | AdminOnly | Actualizar empresa |

### User (Usuarios)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/User/first-register` | No | Registro del primer usuario (bootstrap) |
| `POST` | `/api/User/register` | AdminOnly | Crear usuario dentro del tenant |
| `POST` | `/api/User/login` | No | Iniciar sesión |
| `GET` | `/api/User/me` | Autorizado | Obtener usuario actual |
| `POST` | `/api/User/refresh` | No (cookies) | Renovar access token |
| `POST` | `/api/User/logout` | Autorizado | Cerrar sesión |
| `GET` | `/api/User/{userId}` | Autorizado | Obtener usuario por ID |
| `GET` | `/api/User` | AdminOrSupervisor | Listar todos los usuarios del tenant |
| `PATCH` | `/api/User/{userId}/role` | AdminOnly | Cambiar rol de usuario |
| `PATCH` | `/api/User/{userId}/status` | AdminOnly | Activar/desactivar usuario |
| `POST` | `/api/User/forgot-password` | No | Enviar código de restablecimiento |
| `POST` | `/api/User/reset-password` | No | Restablecer contraseña |

### Role (Roles)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/Role` | AdminOrSupervisor | Listar roles del tenant |
| `GET` | `/api/Role/{roleId}` | AdminOrSupervisor | Obtener rol por ID |

### Product (Productos)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/Product` | ReportsAccess | Listar productos |
| `GET` | `/api/Product/{id}` | ReportsAccess | Obtener producto por ID |
| `POST` | `/api/Product` | InventoryManagement | Crear producto |
| `PUT` | `/api/Product/{id}` | InventoryManagement | Actualizar producto |
| `GET` | `/api/Product/filter` | ReportsAccess | Filtrar productos |
| `PATCH` | `/api/Product/price` | InventoryManagement | Actualizar precio |
| `GET` | `/api/Product/with-photos` | ReportsAccess | Productos con fotos |

### ProductCategory (Categorías)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/ProductCategory` | ReportsAccess | Listar categorías |
| `GET` | `/api/ProductCategory/{id}` | ReportsAccess | Obtener categoría |
| `POST` | `/api/ProductCategory` | InventoryManagement | Crear categoría |

### Customer (Clientes)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/Customer` | SalesAccess | Listar clientes |
| `GET` | `/api/Customer/{id}` | SalesAccess | Obtener cliente |
| `POST` | `/api/Customer` | SalesAccess | Crear cliente |

### Supplier (Proveedores)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/Supplier` | InventoryManagement | Listar proveedores |
| `GET` | `/api/Supplier/{supplierId}` | InventoryManagement | Obtener proveedor |
| `POST` | `/api/Supplier` | InventoryManagement | Crear proveedor |
| `PUT` | `/api/Supplier/{supplierId}` | InventoryManagement | Actualizar proveedor |

### Sale (Ventas)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/Sale` | SalesAccess | Crear venta |
| `GET` | `/api/Sale/{saleId}` | SalesAccess | Obtener venta |
| `GET` | `/api/Sale` | SalesAccess | Listar ventas |
| `GET` | `/api/Sale/ByDateRange` | SalesAccess | Ventas por rango de fechas |
| `GET` | `/api/Sale/ByCustomer/{customerId}` | SalesAccess | Ventas por cliente |
| `GET` | `/api/Sale/History` | SalesAccess | Historial de ventas |
| `GET` | `/api/Sale/{saleId}/InvoicePdf` | SalesAccess | Descargar factura PDF |
| `GET` | `/api/Sale/ReportPdf` | SalesAccess | Reporte PDF de ventas |
| `GET` | `/api/Sale/DailyInvoicesPdf` | SalesAccess | Facturas del día (PDF) |

### Purchase (Compras)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/Purchase` | InventoryManagement | Crear compra |
| `GET` | `/api/Purchase` | ReportsAccess | Listar compras |
| `GET` | `/api/Purchase/{purchaseId}` | ReportsAccess | Obtener compra |

### Dashboard

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/dashboard/summary` | ReportsAccess | Resumen del dashboard |
| `GET` | `/api/dashboard/top-products` | ReportsAccess | Productos más vendidos |
| `GET` | `/api/dashboard/sales-analytics` | ReportsAccess | Analytics de ventas |

### Report (Reportes)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/Report/sales-by-day` | ReportsAccess | Ventas por día |
| `GET` | `/api/Report/sales-by-product` | ReportsAccess | Ventas por producto |
| `GET` | `/api/Report/sales-by-category` | ReportsAccess | Ventas por categoría |
| `GET` | `/api/Report/profits` | ReportsAccess | Ganancias |
| `GET` | `/api/Report/inventory` | ReportsAccess | Inventario |
| `GET` | `/api/Report/purchase-history` | ReportsAccess | Historial de compras |

### StockAlert (Alertas de Inventario)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/StockAlert/refresh` | ReportsAccess | Recalcular alertas |
| `GET` | `/api/StockAlert` | ReportsAccess | Obtener alertas activas |
| `PATCH` | `/api/StockAlert/{alertId}/resolve` | ReportsAccess | Resolver alerta |

### PaymentMethod (Métodos de Pago)

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/PaymentMethod` | SalesAccess | Listar métodos de pago |
| `GET` | `/api/PaymentMethod/{id}` | SalesAccess | Obtener método de pago |

### AI Copilot

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/AICopilot/chat` | Autorizado | Enviar mensaje al copiloto |
| `GET` | `/api/AICopilot/context/{conversationId}` | Autorizado | Obtener contexto de conversación |
| `DELETE` | `/api/AICopilot/context/{conversationId}` | Autorizado | Eliminar contexto |
| `GET` | `/api/AICopilot/health` | No | Verificar estado del AI |

---

## 7. Códigos de Respuesta HTTP

| Código | Significado | Cuándo ocurre |
|--------|-------------|---------------|
| `200` | OK | Operación exitosa |
| `201` | Created | Recurso creado exitosamente (onboarding, register, create) |
| `204` | No Content | Eliminación exitosa sin cuerpo de respuesta |
| `400` | Bad Request | Datos de entrada inválidos o validación fallida |
| `401` | Unauthorized | No autenticado o token expirado |
| `403` | Forbidden | Autenticado pero sin permisos suficientes |
| `404` | Not Found | Recurso no encontrado |
| `409` | Conflict | Recurso duplicado (slug, email, username ya existen) |
| `500` | Internal Server Error | Error inesperado del servidor |

### Formato de Errores

Todos los errores siguen este formato:

```json
{
  "statusCode": 400,
  "message": "Descripción del error en español",
  "traceId": "0HNN12F826CRH:00000003",
  "timestamp": "2026-07-13T19:20:46.3520856Z"
}
```

---

## 8. Guía de Integración Frontend

### Flujo Completo: Onboarding → Login → Uso

#### Paso 1: Crear Empresa (Onboarding)

```javascript
// POST /api/Tenant/onboard
async function onboardCompany(tenantData, adminData) {
  const response = await fetch(`${API_URL}/api/Tenant/onboard`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      tenant: tenantData,
      administrator: adminData
    })
  });

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result.message);
  }

  return result;
  // { tenantId, tenantName, administratorUserId, administratorUsername, message }
}
```

#### Paso 2: Login

```javascript
// POST /api/User/login
async function login(username, password) {
  const response = await fetch(`${API_URL}/api/User/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include', // CRÍTICO: para recibir cookies
    body: JSON.stringify({ username, password })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }

  const user = await response.json();
  // Las cookies se guardan automáticamente
  return user;
  // { userId, fullName, username, roleName, tenantId, tenantName }
}
```

#### Paso 3: Peticiones Autenticadas

```javascript
// Todas las peticiones autenticadas necesitan credentials: 'include'
async function apiGet(endpoint) {
  const response = await fetch(`${API_URL}${endpoint}`, {
    method: 'GET',
    credentials: 'include' // Las cookies se envían automáticamente
  });

  if (response.status === 401) {
    // Token expirado → intentar refresh
    const refreshed = await refreshToken();
    if (refreshed) {
      return apiGet(endpoint); // Reintentar
    }
    window.location.href = '/login';
    return;
  }

  return await response.json();
}
```

#### Paso 4: Refresh Token Automático

```javascript
// Interceptor de fetch que renueva tokens automáticamente
async function fetchWithRefresh(url, options = {}) {
  let response = await fetch(url, {
    ...options,
    credentials: 'include'
  });

  if (response.status === 401) {
    // Intentar refresh
    const refreshResponse = await fetch(`${API_URL}/api/User/refresh`, {
      method: 'POST',
      credentials: 'include'
    });

    if (refreshResponse.ok) {
      // Reintentar la petición original
      response = await fetch(url, {
        ...options,
        credentials: 'include'
      });
    } else {
      // Refresh falló → login
      window.location.href = '/login';
      return;
    }
  }

  return response;
}

// Uso
const data = await fetchWithRefresh(`${API_URL}/api/Product`);
const products = await data.json();
```

#### Paso 5: Logout

```javascript
async function logout() {
  await fetch(`${API_URL}/api/User/logout`, {
    method: 'POST',
    credentials: 'include'
  });
  window.location.href = '/login';
}
```

### Ejemplo Completo: Crear Empresa y Hacer Login

```javascript
async function setupCompany() {
  try {
    // 1. Onboarding
    const onboardResult = await onboardCompany(
      {
        name: "Mi Empresa",
        slug: "mi-empresa",
        contactEmail: "info@miempresa.com",
        phoneNumber: "+50370001111",
        address: "San Salvador",
        maxUsers: 50,
        maxProducts: 5000
      },
      {
        fullName: "Admin General",
        username: "admin",
        email: "admin@miempresa.com",
        password: "Admin123*"
      }
    );

    console.log(`Empresa creada: ${onboardResult.tenantName} (ID: ${onboardResult.tenantId})`);

    // 2. Login automático
    const loginResult = await login("admin", "Admin123*");
    console.log(`Bienvenido, ${loginResult.fullName}!`);

    // 3. Ahora puedes hacer peticiones autenticadas
    const roles = await fetchWithRefresh(`${API_URL}/api/Role`);
    console.log("Roles:", await roles.json());

  } catch (error) {
    console.error("Error:", error.message);
  }
}
```

### Manejo de Errores por Campo

Cuando la API retorna errores de validación (400), el frontend puede mostrar mensajes específicos por campo:

```javascript
async function handleApiError(response) {
  const data = await response.json();

  if (response.status === 400) {
    // Errores de validación de ModelState
    if (data.errors) {
      // Formato: { "errors": { "Tenant.Name": ["El nombre es requerido"] } }
      Object.entries(data.errors).forEach(([field, messages]) => {
        console.error(`${field}: ${messages.join(', ')}`);
      });
    } else {
      // Error de negocio simple
      console.error(data.message);
    }
  } else if (response.status === 401) {
    window.location.href = '/login';
  } else if (response.status === 409) {
    console.error("Conflicto:", data.message);
  } else {
    console.error("Error del servidor:", data.message);
  }
}
```

### Configuración de la URL Base

```javascript
// config.js
const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5213';

export default API_URL;
```

### Constantes de la API

```javascript
// constants.js
export const ROLES = {
  ADMINISTRATOR: 'Administrador',
  SUPERVISOR: 'Supervisor',
  INVENTORY_MANAGER: 'Encargado de inventario',
  CASHIER: 'Cajero'
};

export const POLICIES = {
  ADMIN_ONLY: 'AdminOnly',
  ADMIN_OR_SUPERVISOR: 'AdminOrSupervisor',
  INVENTORY: 'InventoryManagement',
  SALES: 'SalesAccess',
  REPORTS: 'ReportsAccess'
};

export const PASSWORD_RULES = {
  MIN_LENGTH: 8,
  REQUIRE_UPPERCASE: true,
  REQUIRE_LOWERCASE: true,
  REQUIRE_DIGIT: true,
  REQUIRE_SPECIAL: true
};
```

---

## Swagger UI

La documentación interactiva está disponible en:

```
http://localhost:5213/
```

El spec OpenAPI (JSON) está en:

```
http://localhost:5213/swagger/v1/swagger.json
```

---

## Notas para el Desarrollador Frontend

1. **Siempre enviar `credentials: 'include'`** en todas las peticiones fetch para que las cookies se envíen y reciban correctamente.

2. **El access token NO se almacena en localStorage**. Se maneja exclusivamente via HttpOnly cookies (más seguro).

3. **El refresh automático** debe implementarse con un interceptor que detecte respuestas 401 y renueve el token antes de reintentar.

4. **El tenant_id** está contenido en el JWT. Si necesitas saber el tenant actual sin hacer un request, puedes decodificar el JWT (aunque no lo recomiendo — mejor usar `/api/User/me`).

5. **Los endpoints de Tenant y User que dicen "AllowAnonymous"** no requieren autenticación. Todos los demás requieren el header `Authorization: Bearer <token>` o la cookie `access_token`.

6. **El onboarding endpoint es público** pero solo debería usarse una vez por empresa. Considera agregar rate limiting en el frontend.

7. **La contraseña del admin** nunca se devuelve en ninguna respuesta de la API.
