using Galaxium.Api.Services.AI.Interfaces;
using Galaxium.Api.Services.AI.Models;

namespace Galaxium.Api.Services.AI.Core;

public class PromptBuilder : IIntentParser
{
    private readonly IAIProvider _aiProvider;
    private readonly ILogger<PromptBuilder> _logger;

    private const string SystemInstruction = @"
Eres el copiloto inteligente del ERP Galaxium. Tu trabajo es interpretar preguntas ambiguas 
del usuario sobre su negocio y traducirlas en llamadas estructuradas a herramientas internas.

REGLAS ABSOLUTAS:
1. NUNCA inventes datos o números.
2. NUNCA generes SQL directamente.
3. NUNCA respondas con información que no venga de una herramienta.
4. USA SIEMPRE la herramienta GetBusinessMetric para consultar datos.
5. Si la intención es ambigua, pregunta al usuario para clarificar.
6. Responde SIEMPRE en idioma español.
7. Convierte fechas en lenguaje natural:
   - 'hoy' → today
   - 'ayer' → yesterday
   - 'esta semana' → current_week
   - 'semana pasada' → last_week
   - 'este mes' → current_month
   - 'mes pasado' → last_month
   - 'últimos 7 días' → last_7_days
   - 'últimos 30 días' → last_30_days
8. Si el usuario dice 'compáralo' o 'comparado con', agrega el parámetro 'comparison'.
9. Los valores de metric aceptados son: sales, profit, customers, products, inventory.

RESPUESTA OBLIGATORIA:
Debes responder ÚNICAMENTE con JSON válido en este formato exacto:
{
  ""tool"": ""GetBusinessMetric"",
  ""args"": {
    ""metric"": ""sales|profit|customers|products|inventory"",
    ""rangeType"": ""today|yesterday|current_week|last_week|current_month|last_month|last_7_days|last_30_days|last_90_days|custom"",
    ""startDate"": ""YYYY-MM-DD (solo si rangeType=custom)"",
    ""endDate"": ""YYYY-MM-DD (solo si rangeType=custom)"",
    ""groupBy"": ""day|week|month|category|product (opcional)"",
    ""comparison"": ""previous_week|previous_month|previous_year (opcional)""
  },
  ""confidence"": 0.0-1.0,
  ""is_complete"": true|false,
  ""clarification_needed"": ""pregunta si falta información importante""
}

EJEMPLOS DE CONVERSACIÓN:

Usuario: dame las ventas de este mes
Respuesta:
{
  ""tool"": ""GetBusinessMetric"",
  ""args"": {
    ""metric"": ""sales"",
    ""rangeType"": ""current_month""
  },
  ""confidence"": 0.95,
  ""is_complete"": true
}

Usuario: y de ayer
Respuesta (reutiliza contexto previo):
{
  ""tool"": ""GetBusinessMetric"",
  ""args"": {
    ""metric"": ""sales"",
    ""rangeType"": ""yesterday""
  },
  ""confidence"": 0.9,
  ""is_complete"": true
}

Usuario: compáralo con la semana pasada
Respuesta (agrega comparación):
{
  ""tool"": ""GetBusinessMetric"",
  ""args"": {
    ""metric"": ""sales"",
    ""rangeType"": ""current_week"",
    ""comparison"": ""previous_week""
  },
  ""confidence"": 0.9,
  ""is_complete"": true
}

Usuario: cuáles fueron las ganancias del mes pasado
Respuesta:
{
  ""tool"": ""GetBusinessMetric"",
  ""args"": {
    ""metric"": ""profit"",
    ""rangeType"": ""last_month""
  },
  ""confidence"": 0.95,
  ""is_complete"": true
}

Usuario: qué productos se vendieron más la semana pasada
Respuesta:
{
  ""tool"": ""GetBusinessMetric"",
  ""args"": {
    ""metric"": ""sales"",
    ""rangeType"": ""last_week"",
    ""groupBy"": ""product""
  },
  ""confidence"": 0.9,
  ""is_complete"": true
}

Usuario: cómo está mi inventario
Respuesta:
{
  ""tool"": ""GetBusinessMetric"",
  ""args"": {
    ""metric"": ""inventory"",
    ""rangeType"": ""current_month""
  },
  ""confidence"": 0.9,
  ""is_complete"": true
}

IMPORTANTE: 
- Si el usuario pregunta algo que no puedas responder con las herramientas disponibles, 
  indica que no tienes esa capacidad.
- Si falta información crítica, establece is_complete: false y usa clarification_needed.
- El campo 'args' debe contener SOLO los argumentos del tool seleccionado.
";

    public PromptBuilder(IAIProvider aiProvider, ILogger<PromptBuilder> logger)
    {
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<IntentResolution> ParseAsync(
        string userMessage,
        ConversationState? previousContext,
        string toolsSchema,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contextInfo = BuildContextInfo(previousContext);
            var fullPrompt = $"{SystemInstruction}\n\n{contextInfo}";

            _logger.LogDebug("Sending prompt to AI for message: {Message}", userMessage);

            var response = await _aiProvider.GenerateContentAsync(
                fullPrompt,
                userMessage,
                cancellationToken);

            _logger.LogDebug("AI Response: {Response}", response);

            return ParseAIResponse(response, userMessage);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429") || ex.Message.Contains("TooManyRequests"))
        {
            _logger.LogWarning("Rate limit hit, using fallback pattern matching");
            return FallbackParsing(userMessage.ToLower(), userMessage);
        }
        catch (Exception ex)
        {
            var errorMsg = ex.Message.ToLower();
            if (errorMsg.Contains("429") || errorMsg.Contains("rate limit") || errorMsg.Contains("quota") || 
                errorMsg.Contains("toomanyrequests") || errorMsg.Contains("expired") || errorMsg.Contains("api key"))
            {
                _logger.LogWarning("Rate limit, quota, or API key error, using fallback pattern matching: {Error}", ex.Message);
                return FallbackParsing(userMessage.ToLower(), userMessage);
            }

            _logger.LogError(ex, "Error parsing intent from message: {Message}", userMessage);
            return new IntentResolution
            {
                Tool = string.Empty,
                Args = new Dictionary<string, object?>(),
                RawText = userMessage,
                Confidence = 0,
                IsComplete = false,
                Error = $"Error al procesar el mensaje: {ex.Message}"
            };
        }
    }

    private string BuildContextInfo(ConversationState? context)
    {
        if (context == null)
            return "CONTEXTO: Esta es la primera pregunta del usuario. No hay contexto previo.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("CONTEXTO CONVERSACIONAL PREVIO:");
        sb.AppendLine($"- Última métrica consultada: {context.LastMetric ?? "ninguna"}");
        sb.AppendLine($"- Último rango temporal: {context.LastRangeType ?? "ninguno"}");

        if (context.LastRangeStart.HasValue && context.LastRangeEnd.HasValue)
        {
            sb.AppendLine($"- Rango de fechas: {context.LastRangeStart:yyyy-MM-dd} a {context.LastRangeEnd:yyyy-MM-dd}");
        }

        if (!string.IsNullOrEmpty(context.LastGroupBy))
            sb.AppendLine($"- Agrupación anterior: {context.LastGroupBy}");

        if (!string.IsNullOrEmpty(context.LastComparison))
            sb.AppendLine($"- Comparación activa: {context.LastComparison}");

        sb.AppendLine();
        sb.AppendLine("El usuario puede referirse a 'eso', 'lo mismo', 'compáralo', etc. - reutiliza el contexto.");

        return sb.ToString();
    }

    private IntentResolution ParseAIResponse(string response, string originalMessage)
    {
        try
        {
            var cleanJson = ExtractJson(response);
            if (string.IsNullOrEmpty(cleanJson))
            {
                return FallbackParsing(response, originalMessage);
            }

            using var doc = System.Text.Json.JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;

            var intent = new IntentResolution
            {
                Tool = GetStringProperty(root, "tool") ?? string.Empty,
                RawText = originalMessage,
                Confidence = GetDoubleProperty(root, "confidence") ?? 0.5,
                IsComplete = GetBoolProperty(root, "is_complete") ?? true,
                ClarificationNeeded = GetStringProperty(root, "clarification_needed"),
                Error = GetStringProperty(root, "error")
            };

            if (root.TryGetProperty("args", out var argsElement) && argsElement.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                foreach (var prop in argsElement.EnumerateObject())
                {
                    var value = ParseJsonElement(prop.Value);
                    intent.Args[prop.Name] = value;
                }
            }

            return intent;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response as JSON, attempting fallback");
            return FallbackParsing(response, originalMessage);
        }
    }

    private IntentResolution FallbackParsing(string response, string originalMessage)
    {
        var lowerResponse = response.ToLower();

        var metric = DetectMetric(lowerResponse);
        var rangeType = DetectRangeType(lowerResponse);
        var hasComparison = lowerResponse.Contains("compar") || lowerResponse.Contains("compáralo") || lowerResponse.Contains("vs");

        if (string.IsNullOrEmpty(metric))
        {
            return new IntentResolution
            {
                Tool = string.Empty,
                RawText = originalMessage,
                Confidence = 0,
                IsComplete = false,
                Error = "No pude identificar qué métrica necesitas. Intenta ser más específico."
            };
        }

        return new IntentResolution
        {
            Tool = IntentConstants.GetBusinessMetric,
            Args = new Dictionary<string, object?>
            {
                ["metric"] = metric,
                ["rangeType"] = rangeType ?? IntentConstants.RangeCurrentMonth,
                ["comparison"] = hasComparison ? IntentConstants.ComparisonPreviousWeek : null
            },
            RawText = originalMessage,
            Confidence = 0.6,
            IsComplete = true
        };
    }

    private string? DetectMetric(string text)
    {
        if (text.Contains("venta") || text.Contains("vendi") || text.Contains("ingreso") || text.Contains("factura"))
            return IntentConstants.MetricSales;
        if (text.Contains("ganancia") || text.Contains("utilidad") || text.Contains("beneficio") || text.Contains("profit"))
            return IntentConstants.MetricProfit;
        if (text.Contains("cliente") || text.Contains("customer"))
            return IntentConstants.MetricCustomers;
        if (text.Contains("producto") || text.Contains("artículo"))
            return IntentConstants.MetricProducts;
        if (text.Contains("inventario") || text.Contains("stock") || text.Contains("existencia"))
            return IntentConstants.MetricInventory;

        return null;
    }

    private string? DetectRangeType(string text)
    {
        if (text.Contains("hoy") || text.Contains("today"))
            return IntentConstants.RangeToday;
        if (text.Contains("ayer") || text.Contains("yesterday"))
            return IntentConstants.RangeYesterday;
        if (text.Contains("esta semana") || text.Contains("current week"))
            return IntentConstants.RangeCurrentWeek;
        if (text.Contains("semana pasad") || text.Contains("last week"))
            return IntentConstants.RangeLastWeek;
        if (text.Contains("este mes") || text.Contains("current month"))
            return IntentConstants.RangeCurrentMonth;
        if (text.Contains("mes pasad") || text.Contains("last month"))
            return IntentConstants.RangeLastMonth;
        if (text.Contains("último") && (text.Contains("7 día") || text.Contains("7 dia")))
            return IntentConstants.RangeLast7Days;
        if (text.Contains("último") && (text.Contains("30 día") || text.Contains("30 dia")))
            return IntentConstants.RangeLast30Days;
        if (text.Contains("último") && (text.Contains("90 día") || text.Contains("90 dia")))
            return IntentConstants.RangeLast90Days;

        return null;
    }

    private string? ExtractJson(string text)
    {
        var startIdx = text.IndexOf('{');
        var endIdx = text.LastIndexOf('}');

        if (startIdx >= 0 && endIdx > startIdx)
        {
            return text.Substring(startIdx, endIdx - startIdx + 1);
        }

        return null;
    }

    private static string? GetStringProperty(System.Text.Json.JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.String
            ? prop.GetString()
            : null;
    }

    private static double? GetDoubleProperty(System.Text.Json.JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var prop))
        {
            return prop.ValueKind switch
            {
                System.Text.Json.JsonValueKind.Number => prop.GetDouble(),
                System.Text.Json.JsonValueKind.String when double.TryParse(prop.GetString(), out var d) => d,
                _ => null
            };
        }
        return null;
    }

    private static bool? GetBoolProperty(System.Text.Json.JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.True
            ? true
            : element.TryGetProperty(name, out prop) && prop.ValueKind == System.Text.Json.JsonValueKind.False
                ? false
                : null;
    }

    private static object? ParseJsonElement(System.Text.Json.JsonElement element)
    {
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => element.GetString(),
            System.Text.Json.JsonValueKind.Number => element.GetDecimal(),
            System.Text.Json.JsonValueKind.True => true,
            System.Text.Json.JsonValueKind.False => false,
            System.Text.Json.JsonValueKind.Null => null,
            System.Text.Json.JsonValueKind.Array or System.Text.Json.JsonValueKind.Object =>
                System.Text.Json.JsonSerializer.Deserialize<object>(element.GetRawText()),
            _ => element.GetRawText()
        };
    }
}
