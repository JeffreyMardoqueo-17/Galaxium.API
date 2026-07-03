# GalaxiumCopilot - Guía para Entrevistas y Explicaciones Técnicas

## Resumen Simple (30 segundos)

Imagina que tienes un asistente en tu teléfono al que le preguntas "¿Cuánto vendí este mes?" y te responde "Vendiste $125,000 dolares, un 15% más que el mes pasado". Eso es GalaxiumCopilot: un chat de IA que conecta el lenguaje natural con los datos reales de tu negocio, sin que el usuario tenga que saber de bases de datos ni fórmulas complejas.

---

## Explicación para Entrevistas Técnicas

### "¿Qué es Galaxium Copilot y cómo funciona?"

**Respuesta estructurada:**

"GalaxiumCopilot es un módulo de asistente conversacional que implementé en el ERP Galaxium. Su objetivo es permitir que los usuarios consulten información de negocio usando lenguaje natural, sin necesidad de generar reportes manuales o conocer la estructura técnica del sistema."

"El flujo funciona así:
1. El usuario escribe una pregunta en español (ej: 'dame las ventas de ayer')
2. El sistema detecta qué información necesita y en qué período (usando Google Gemini)
3. Traduce eso a una llamada estructurada a una herramienta llamada GetBusinessMetric
4. Esa herramienta usa los servicios existentes del ERP (ISaleService, IReportService, etc.)
5. Los datos vuelven y se convierten a lenguaje natural
6. El contexto se guarda en Redis para que la siguiente pregunta pueda referirse a la anterior"

### "¿Por qué no consultas la base de datos directamente con SQL?"

"Esa fue una decisión arquitectónica muy importante. Había dos opciones:

**Opción A - SQL directo:**
- Ventajas: Rápido, flexible
- Desventajas: Riesgo de SQL injection, acoplamiento a schema, requiere que la IA sepa SQL (lo cual es complejo y costoso), no reutiliza lógica de negocio

**Opción B - Herramientas (lo que implementé):**
- Ventajas: Seguridad total (no hay SQL), reutiliza lógica de negocio existente, más fácil de mantener, los servicios ya tienen permisos y validaciones
- Desventajas: Más complejo de implementar inicialmente

Elegí la Opción B porque en un ERP la seguridad y la reutilización de lógica de negocio son críticas."

### "¿Cómo mantiene el contexto entre conversaciones?"

"Uso Redis como almacenamiento temporal de estado conversacional. Cada conversación tiene una key única basada en `tenantId:userId:conversationId`, y los datos incluyen:
- Última métrica consultada (ventas, ganancias, etc.)
- Último rango de fechas
- Filtros aplicados
- Resultado anterior (para comparaciones)

Ejemplo: Si el usuario pregunta 'dame las ventas' y luego dice 'y de ayer', el sistema sabe que debe reuse 'ventas' pero cambiar el rango a 'ayer', sin que el usuario lo diga explícitamente."

### "¿Qué pasa si Gemini no entiende bien la pregunta?"

"Implementé un sistema de fallback muy robusto:

1. **Parsing primario**: Intento parsear la respuesta JSON de Gemini
2. **Fallback heurístico**: Si falla, uso regex y keywords para detectar la intención
   - Si menciona 'venta' o 'ingreso' → metric=sales
   - Si menciona 'ayer' → rangeType=yesterday
   - Si menciona 'compáralo' → agregar comparison

3. **Solicitar clarificación**: Si realmente no puedo determinar qué necesita, pregunto al usuario amablemente"

### "¿Cómo manejan los permisos multi-tenant?"

"El sistema es completamente multi-tenant:
1. El `tenantId` viene del JWT token del usuario
2. Cada servicio del ERP ya filtra datos por tenant
3. El contexto en Redis también está aislado por tenant
4. No hay forma de que un usuario vea datos de otro tenant"

---

## Arquitectura Explicada para No-Técnicos

### "¿Cómo le explicas a tu abuela cómo funciona?"

"Imagina que tienes unmayordomo virtual que trabaja en tu tienda.

**Sin GalaxiumCopilot:**
- Tú: 'Necesito saber cuánto vendí'
- Mayordomo: '¿Qué significa vender? ¿Quieres las ventas de hoy? ¿De la semana? ¿En efectivo o tarjeta?'
- Tú: 'Las de este mes, en efectivo'
- Mayordomo: '¿Este mes meaning? ¿Enero? ¿Febrero? ¿Marzo?'
- Tú: 🤬

**Con GalaxiumCopilot:**
- Tú: '¿Cuánto vendí este mes?'
- GalaxiumCopilot: '¡Claro! Vendiste $125,000 dolares en marzo. Eso es un 15% más que febrero.'
- Tú: '¿Y de la semana pasada?'
- GalaxiumCopilot: 'La semana pasada vendiste $28,000 dolares, un 5% menos que la semana anterior.'

El mayordomo (GalaxiumCopilot) recuerda lo que hablamos antes (vender = metric) y solo pregunta por lo que cambió (la semana)."

---

## Preguntas Frecuentes en Entrevistas

### "Diseño del sistema"

**P: ¿Cómo diseñas un sistema que convierte lenguaje natural a acciones?**

"R: Usando el patrón de **Tool Orchestration** o **Function Calling**. El flujo es:
1. **Parser**: Convierte texto → intención estructurada
2. **Validator**: Verifica que la intención sea completa y válida
3. **Executor**: Ejecuta la acción correspondiente
4. **Formatter**: Convierte el resultado a formato legible

Cada paso es un componente separado que puede probarse independientemente (principio de responsabilidad única)."

**P: ¿Cómo manejas la latencia?**

"R: En tres niveles:
1. **Timeouts**: Gemini tiene timeout de 10 segundos
2. **Caché**: Consultas idénticas pueden cachearse (para futura implementación)
3. **Async**: Todo es asíncrono, no bloqueamos el request thread"

### "Patrones de diseño utilizados"

**P: ¿Qué patrones de diseño aplicaste?**

"R: Varios:

1. **Strategy Pattern** para `IAIProvider`:
   - Permite cambiar entre Gemini, OpenAI, etc. sin cambiar código del consumidor
   ```csharp
   public interface IAIProvider {
       Task<string> GenerateContentAsync(...);
   }
   ```

2. **Registry Pattern** para `IToolRegistry`:
   - Centraliza las herramientas disponibles
   - Permite agregar nuevas herramientas sin modificar el ejecutor

3. **Facade Pattern** en `GalaxiumCopilotService`:
   - Oculta la complejidad de coordinar múltiples servicios
   - El cliente solo ve un método simple: `ProcessMessageAsync`

4. **Repository Pattern** (implícito) en `IConversationContextStore`:
   - Abstrae cómo se persiste el contexto
   - Hoy es Redis, mañana podría ser PostgreSQL sin cambiar código

5. **Factory Pattern** en `ToolExecutor`:
   - Crea la instancia correcta según el tipo de métrica"

### "Manejo de errores"

**P: ¿Qué pasa si Gemini está caído?**

"R: El sistema maneja graceful degradation:
1. Timeout en la llamada HTTP (10 segundos)
2. Si falla, retorno error amigable al usuario
3. Los logs capturan el error completo para debugging
4. No expongo errores internos de infraestructura al cliente"

**P: ¿Cómo manejas casos ambiguos?**

"R: Tengo tres estrategias:
1. **Usar contexto previo**: Si el usuario dice 'compáralo', asumo que quiere comparar lo mismo que acaba de preguntar
2. **Asumir defaults sensatos**: Si no dice rango, uso 'este mes'
3. **Preguntar**: Si realmente no puedo saber, pido clarificación educadamente"

### "Seguridad"

**P: ¿Cómo proteges contra inyecciones en los prompts?**

"R: Buenas preguntas:
1. **Validación de entrada**: Los DTOs tienen Data Annotations
2. **No ejecutamos SQL**: El prompt nunca llega a la base de datos
3. **Prompt injection**: Los prompts de sistema son fixed, solo el mensaje del usuario va como user input
4. **Sanitización básica**: Eliminamos caracteres potencialmente problemáticos"

**P: ¿Cómo manejas la autorización?**

"R: Completamente integrado con JWT:
1. Todos los endpoints requieren `[Authorize]`
2. El `tenantId` y `userId` vienen del claims del JWT
3. Los servicios subyacentes ya filtran por estos IDs
4. Redis keys incluyen tenantId para aislamiento"

---

## Conceptos Clave para Entender el Sistema

### 1. Tool Calling / Function Calling

Es una técnica donde la IA no solo genera texto, sino que "llama funciones" con parámetros estructurados.

```
Usuario: "dame las ventas"

IA genera (no como texto, sino como tool call):
{
  "tool": "GetBusinessMetric",
  "args": {
    "metric": "sales",
    "rangeType": "current_month"
  }
}
```

Esto es más confiable que hacer que la IA genere SQL o respuestas numéricas directamente.

### 2. Conversational Context

El sistema mantiene "memoria" de la conversación para poder entender referencias como:
- "también" → reuse la métrica anterior
- "compáralo" → agregar comparación
- "de ayer" → cambiar solo el rango de fechas

Sin contexto, "de ayer" no tendría sentido. Con contexto, sabe que se refiere a "ventas de ayer".

### 3. Fallback Strategy

Siempre hay un plan B. Si el parser principal falla:
1. Intentar parsing heurístico
2. Si falla, error amigable
3. Nunca fallar silenciosamente

### 4. Multi-Tenancy

El sistema sirve a múltiples empresas (tenants) con datos completamente aislados. Cada query de base de datos incluye `WHERE tenant_id = X`.

---

## Código de Ejemplo Explicable

### El flujo más simple posible

```csharp
// 1. Usuario llama a esto
public async Task<AIChatResponseDto> Chat(AIChatRequestDto request)
{
    // 2. Obtener contexto anterior (o null si es primera vez)
    var context = await _contextStore.GetAsync(request.TenantId, request.UserId);
    
    // 3. Pedirle a Gemini que interprete el mensaje
    var intent = await _aiProvider.Interpret(request.Message, context);
    
    // 4. Ejecutar la herramienta que Gemini decidió
    var result = await _toolExecutor.Execute(intent.Tool, intent.Args);
    
    // 5. Convertir resultado a texto legible
    var response = _formatter.Format(result);
    
    // 6. Guardar contexto para la próxima vez
    await _contextStore.SaveAsync(request.TenantId, request.UserId, intent.NewState);
    
    return response;
}
```

### Por qué esto es mantenible

1. Cada paso es una clase separada
2. Cada clase tiene una responsabilidad clara
3. Puedo probar cada paso independientemente
4. Puedo cambiar Gemini por OpenAI cambiando una línea
5. Puedo agregar herramientas sin modificar el ejecutor

---

## Metáforas para Explicar

| Concepto Técnico | Metáfora |
|-------------------|----------|
| Tool Calling | El chef que lee una receta y sabe qué ingredientes usar |
| Conversational Context | El mozo que recuerda tu pedido anterior |
| Multi-Tenant | Cada tenant es un edificio separado con su propia recepción |
| Fallback | El GPS que si no encuentra la ruta, propone alternatives |
| IAIProvider | El traductor que puede hablar español, inglés o mandarín |

---

## Para Profundizar

### Temas avanzados (para entrevistas senior):

1. **Prompt Engineering**: Cómo diseñé el system prompt para guiar a Gemini
2. **Cost Optimization**: Gemini es más barato que GPT-4, y el caching reduce costos
3. **Testing Strategy**: Unit tests del parser, integración con servicios mockeados
4. **Monitoring**: Logs estructurados, métricas de latencia y errores
5. **Future Enhancements**: Predicciones, recomendaciones, chat histórico

### Preguntas de seguimiento comunes:

- "¿Cómo escalarías esto a 1000 usuarios concurrentes?"
- "¿Qué harías si Gemini cambia su API?"
- "¿Cómo testearías que las comparaciones son correctas?"
- "¿Por qué elegiste Gemini sobre OpenAI?"

---

## Checklist para Entrevistas

- [x] Explicar el problema que resuelve (2 minutos)
- [x] Describir el flujo de datos (3 minutos)
- [x] Justificar decisiones arquitectónicas (3 minutos)
- [x] Hablar de patrones de diseño (2 minutos)
- [x] Discutir manejo de errores (2 minutos)
- [x] Explicar seguridad y multi-tenancy (2 minutos)
- [x] Responder preguntas de seguimiento (5+ minutos)
