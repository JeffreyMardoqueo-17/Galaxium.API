# AICopilot - Arquitectura Técnica Completa

## Tabla de Contenidos
1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Flujo de Conversación](#flujo-de-conversación)
4. [Componentes Principales](#componentes-principales)
5. [Catálogo de Herramientas](#catálogo-de-herramientas)
6. [API Reference](#api-reference)
7. [Configuración](#configuración)
8. [Escalabilidad y Rendimiento](#escalabilidad-y-rendimiento)
9. [Seguridad](#seguridad)
10. [Diagrama de Arquitectura](#diagrama-de-arquitectura)

---

## Resumen Ejecutivo

El módulo **AICopilot** es un asistente conversacional de inteligencia artificial integrado al ERP Galaxium que permite a los usuarios consultar información de su negocio mediante lenguaje natural, sin necesidad de escribir prompts técnicos o conocer la estructura de la base de datos.

### Objetivos Principales
- Interpretar preguntas ambiguas en español
- Convertir lenguaje natural a llamadas estructuradas a herramientas internas
- Mantener contexto conversacional entre mensajes
- Reutilizar servicios existentes del ERP (principio DRY)
- Soportar arquitectura multi-tenant

### Tecnologías Utilizadas
- **ASP.NET Core 8.0** - Framework principal
- **Google Gemini API** - Motor de IA para interpretación de lenguaje natural
- **Redis** - Almacenamiento de memoria conversacional
- **Entity Framework Core** - Acceso a datos (vía servicios existentes)

---

## Arquitectura del Sistema

### Estructura de Carpetas

```
Galaxium.Api/
├── Controllers/
│   └── AICopilotController.cs          # Endpoint REST del chat
│
├── DTOs/AI/
│   ├── AIChatRequestDto.cs            # Request del chat
│   ├── AIChatResponseDto.cs           # Response del chat
│   ├── ToolCallDto.cs                 # Llamada a herramienta
│   ├── ToolResultDto.cs               # Resultado de herramienta
│   └── ConversationContextDto.cs      # Contexto de conversación
│
└── Services/AI/
    ├── Interfaces/
    │   ├── IAICopilotService.cs       # Servicio principal
    │   ├── IAIProvider.cs              # Abstracción del motor de IA
    │   ├── IConversationContextStore.cs # Memoria conversacional
    │   ├── IToolRegistry.cs            # Registro de herramientas
    │   ├── IToolExecutor.cs            # Ejecutor de herramientas
    │   ├── IIntentParser.cs            # Parser de intenciones
    │   └── IResponseFormatter.cs       # Formateador de respuestas
    │
    ├── Models/
    │   ├── ConversationState.cs        # Estado de conversación
    │   ├── ToolDefinition.cs           # Definición de herramienta
    │   ├── ToolExecutionResult.cs      # Resultado de ejecución
    │   └── IntentResolution.cs         # Resolución de intención
    │
    ├── Context/
    │   └── RedisConversationContextStore.cs  # Implementación Redis
    │
    ├── Core/
    │   ├── AICopilotService.cs        # Servicio principal
    │   ├── GeminiProvider.cs           # Implementación Gemini
    │   ├── ToolRegistry.cs             # Registro de herramientas
    │   ├── ToolExecutor.cs             # Ejecutor de herramientas
    │   ├── PromptBuilder.cs            # Constructor de prompts
    │   └── ResponseFormatter.cs        # Formateador de respuestas
    │
    └── AICopilotService.cs             # Orquestador principal
```

---

## Flujo de Conversación

### Diagrama de Secuencia

```
┌─────────┐     ┌──────────────┐     ┌──────────────┐     ┌────────────┐
│ Cliente │     │  Controller  │     │ AICopilot   │     │   Gemini   │
└────┬────┘     └──────┬───────┘     │   Service   │     │  Provider  │
     │                 │             └──────┬───────┘     └─────┬──────┘
     │ POST /chat      │                    │                   │
     │────────────────>│                    │                   │
     │                 │ ProcessMessageAsync│                   │
     │                 │──────────────────>│                   │
     │                 │                   │                   │
     │                 │                   │ GetContext (Redis) │
     │                 │                   │──────────>│        │
     │                 │                   │<──────────│        │
     │                 │                   │                   │
     │                 │                   │ ParseIntent        │
     │                 │                   │──────────────────>│
     │                 │                   │                   │
     │                 │                   │     JSON Response │
     │                 │                   │<──────────────────│
     │                 │                   │                   │
     │                 │                   │ ExecuteTool       │
     │                 │                   │──┐                │
     │                 │                   │  │ (SaleService,  │
     │                 │                   │  │  ReportService,│
     │                 │                   │  │  etc.)        │
     │                 │                   │<─┘                │
     │                 │                   │                   │
     │                 │                   │ FormatResponse     │
     │                 │                   │                   │
     │                 │                   │ SaveContext (Redis)│
     │                 │                   │──────────>│        │
     │                 │                   │                   │
     │                 │ Response          │                   │
     │<────────────────│<─────────────────│                   │
     │ 200 OK + Data   │                  │                   │
```

### Pasos del Flujo

1. **Recibir mensaje** - El controlador recibe el mensaje del usuario
2. **Obtener contexto** - Se consulta Redis para recuperar el estado previo de la conversación
3. **Parsear intención** - Se envía el mensaje + contexto a Gemini para interpretar la intención
4. **Ejecutar herramienta** - Se ejecuta la herramienta seleccionada usando los servicios existentes
5. **Formatear respuesta** - Se convierte el resultado a lenguaje natural
6. **Guardar contexto** - Se persiste el nuevo estado en Redis para la siguiente interacción

---

## Componentes Principales

### 1. AICopilotService (Orquestador)

**Responsabilidad**: Coordina todo el flujo de procesamiento del mensaje.

```csharp
public interface IAICopilotService
{
    Task<AIChatResponseDto> ProcessMessageAsync(
        AIChatRequestDto request, 
        CancellationToken ct = default);
    
    Task<ConversationContextDto?> GetConversationContextAsync(...);
    
    Task ClearConversationAsync(...);
}
```

**Patrones aplicados**:
- **Facade Pattern**: Oculta la complejidad del sistema tras una interfaz simple
- **Orchestrator Pattern**: Coordina múltiples servicios

### 2. IAIProvider (Abstracción de IA)

**Responsabilidad**: Abstrae el proveedor de IA para permitir cambios futuros.

```csharp
public interface IAIProvider
{
    string ProviderName { get; }
    string Model { get; }
    
    Task<string> GenerateContentAsync(
        string systemInstruction, 
        string userMessage, 
        CancellationToken ct = default);
}
```

**Implementaciones actuales**:
- `GeminiProvider` - Google Gemini API

### 3. IConversationContextStore (Memoria)

**Responsabilidad**: Gestiona el estado conversacional persistente.

```csharp
public interface IConversationContextStore
{
    Task<ConversationState?> GetContextAsync(
        string tenantId, 
        string userId, 
        string conversationId);
    
    Task SaveContextAsync(...);
    Task ClearContextAsync(...);
}
```

**Key Pattern**: `ai:context:{tenantId}:{userId}:{conversationId}`

**TTL**: 30 minutos (configurable)

### 4. IToolRegistry (Catálogo)

**Responsabilidad**: Mantiene el registro de herramientas disponibles.

```csharp
public interface IToolRegistry
{
    void RegisterTool(ToolDefinition tool);
    ToolDefinition? GetTool(string toolName);
    string GetToolsSchema();
}
```

### 5. IToolExecutor (Ejecutor)

**Responsabilidad**: Ejecuta las herramientas seleccionadas.

```csharp
public interface IToolExecutor
{
    Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        Dictionary<string, object> arguments,
        int tenantId,
        int userId,
        CancellationToken ct = default);
}
```

---

## Catálogo de Herramientas

### GetBusinessMetric

La herramienta principal que consulta métricas de negocio.

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `metric` | string | Sí | `sales`, `profit`, `customers`, `products`, `inventory` |
| `rangeType` | string | Sí | `today`, `yesterday`, `current_week`, `last_week`, `current_month`, `last_month`, `last_7_days`, `last_30_days`, `last_90_days`, `custom` |
| `startDate` | string | No | `YYYY-MM-DD` (solo para `custom`) |
| `endDate` | string | No | `YYYY-MM-DD` (solo para `custom`) |
| `groupBy` | string | No | `day`, `week`, `month`, `category`, `product` |
| `comparison` | string | No | `previous_week`, `previous_month`, `previous_year` |

### Ejemplo de Llamada

```json
{
  "tool": "GetBusinessMetric",
  "args": {
    "metric": "sales",
    "rangeType": "current_month",
    "comparison": "previous_month"
  }
}
```

---

## API Reference

### Endpoints

#### POST /api/AICopilot/chat

Realiza una consulta conversacional.

**Request:**
```json
{
  "message": "dame las ventas del mes pasado comparadas con el anterior",
  "conversationId": "optativo-genera-uuid",
  "tenantId": 1,
  "userId": 1
}
```

**Response:**
```json
{
  "conversationId": "abc-123",
  "response": "Las ventas totales fueron $125,000.00 MXN. Hubo un incremento del 15.3% comparado con el mes anterior. 📈",
  "requiresClarification": false,
  "data": {
    "metric": "sales",
    "total": 125000.00,
    "formattedValue": "$125,000.00 MXN"
  },
  "metricSummary": {
    "metric": "sales",
    "formattedValue": "$125,000.00 MXN",
    "changeDirection": "up",
    "percentageChange": 15.3,
    "comparisonPeriod": "previous_month"
  },
  "success": true
}
```

#### GET /api/AICopilot/context/{conversationId}

Obtiene el contexto de una conversación.

#### DELETE /api/AICopilot/context/{conversationId}

Limpia el contexto de una conversación.

#### GET /api/AICopilot/health

Verifica el estado del servicio.

---

## Configuración

### Variables de Entorno (Docker Compose)

```yaml
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  api:
    environment:
      Redis__ConnectionString: redis://redis:6379
      AI__GeminiApiKey: ${GEMINI_API_KEY}
      AI__GeminiModel: gemini-2.0-flash
```

### Archivo .env

```env
GEMINI_API_KEY=tu-api-key-de-gemini
GEMINI_MODEL=gemini-2.0-flash
```

### Configuración en Program.cs

```csharp
// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "GalaxiumAI:";
});

// AI Services
builder.Services.AddSingleton<IToolRegistry, ToolRegistry>();
builder.Services.AddScoped<IAIProvider, GeminiProvider>();
builder.Services.AddScoped<IConversationContextStore, RedisConversationContextStore>();
builder.Services.AddScoped<IToolExecutor, ToolExecutor>();
builder.Services.AddScoped<IIntentParser, PromptBuilder>();
builder.Services.AddScoped<IResponseFormatter, ResponseFormatter>();
builder.Services.AddScoped<IAICopilotService, AICopilotService>();
```

---

## Escalabilidad y Rendimiento

### Consideraciones de Rendimiento

1. **Redis**: 
   - Conexiones pooling automático
   - TTL de 30 minutos para cleanup automático
   - Serialización JSON optimizada

2. **API de Gemini**:
   - Temperatura baja (0.3) para respuestas consistentes
   - Límite de tokens (2048) para respuestas predecibles
   - Caché de respuestas para queries idénticas (futuro)

3. **Base de Datos**:
   - Reutiliza servicios existentes con pooling de conexiones
   - Queries optimizadas por los servicios existentes

### Patrones de Escalabilidad

```
                    ┌─────────────────┐
                    │  Load Balancer  │
                    └────────┬────────┘
                             │
          ┌──────────────────┼──────────────────┐
          │                  │                  │
    ┌─────▼─────┐    ┌─────▼─────┐    ┌─────▼─────┐
    │  API Pod 1 │    │  API Pod 2 │    │  API Pod N │
    └─────┬─────┘    └─────┬─────┘    └─────┬─────┘
          │                  │                  │
          └──────────────────┼──────────────────┘
                             │
                    ┌────────▼────────┐
                    │     Redis      │
                    │   (Cluster)    │
                    └────────────────┘
```

---

## Seguridad

### Autenticación y Autorización

- Todos los endpoints requieren JWT Bearer token
- El `tenantId` y `userId` se extraen del token JWT
- Aislamiento de datos por tenant (multi-tenant)

### Validación de Entrada

- Todos los DTOs tienen validación con Data Annotations
- Sanitización de mensajes antes de enviar a Gemini
- Rate limiting por usuario (configurable)

### Protección de Datos

- No se almacenan datos sensibles en Redis
- TTL automático para limpieza de contexto
- Logs sin información sensible

---

## Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           FRONTEND (Next.js)                            │
│                    Chat Interface - Natural Language                   │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │ HTTPS
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         API GATEWAY                                    │
│                   JWT Authentication - CORS                            │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      AICOPILOT MODULE                                  │
│  ┌─────────────────────────────────────────────────────────────────┐  │
│  │                    AICopilotController                         │  │
│  │                    POST /api/AICopilot/chat                    │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                                 │                                      │
│  ┌─────────────────────────────────────────────────────────────────┐  │
│  │                      AICopilotService                          │  │
│  │                    (Orchestrator)                              │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│         │                    │                      │                   │
│         ▼                    ▼                      ▼                   │
│  ┌────────────┐      ┌────────────┐         ┌────────────┐          │
│  │  Prompt    │      │    Tool    │         │  Response   │          │
│  │  Builder   │      │  Executor  │         │  Formatter  │          │
│  └─────┬──────┘      └─────┬──────┘         └────────────┘          │
│        │                    │                                        │
│        ▼                    ▼                                        │
│  ┌────────────┐      ┌────────────────────────────────────┐         │
│  │   Gemini   │      │         Tool Registry               │         │
│  │  Provider  │      │   (GetBusinessMetric tool)        │         │
│  └─────┬──────┘      └──────────────┬───────────────────┘         │
│        │                             │                                │
└────────┼─────────────────────────────┼────────────────────────────────┘
         │                             │
         ▼                             ▼
┌─────────────────┐           ┌────────────────────────────────────────┐
│  Google Gemini  │           │         ERP SERVICES                    │
│      API        │           │  ┌──────────┐  ┌──────────┐             │
└─────────────────┘           │  │   Sale   │  │  Report  │             │
                            │  │  Service │  │  Service │             │
                            │  └────┬─────┘  └────┬─────┘             │
                            │       │              │                   │
                            │  ┌────▼─────┐  ┌────▼─────┐             │
                            │  │ Product  │  │Customer  │             │
                            │  │ Service  │  │ Service  │             │
                            │  └──────────┘  └──────────┘             │
                            └─────────────────┼──────────────────────────┘
                                              │
                            ┌─────────────────┼─────────────────┐
                            ▼                 ▼                 ▼
                     ┌───────────┐     ┌───────────┐    ┌───────────┐
                     │ PostgreSQL │     │   Redis   │    │    NGINX  │
                     │  (Data)   │     │ (Context) │    │ (Reverse) │
                     └───────────┘     └───────────┘    └───────────┘
```

---

## Versiones y Changelog

### v1.0.0 (2026-03-30)
- Implementación inicial del módulo AICopilot
- Integración con Google Gemini API
- Soporte para métricas: ventas, ganancias, clientes, productos, inventario
- Memoria conversacional con Redis
- Formato de respuesta en lenguaje natural
- Comparaciones entre períodos

---

## Autores

- Implementación: AICopilot Team
- Fecha: 30 de Marzo, 2026
- Versión del sistema: Galaxium ERP v2.0
