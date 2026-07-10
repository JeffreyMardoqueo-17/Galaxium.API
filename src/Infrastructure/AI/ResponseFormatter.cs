using System.Text;
using System.Text.Json;
using Galaxium.Api.Services.AI.Interfaces;

namespace Galaxium.Api.Services.AI.Core;

public class ResponseFormatter : IResponseFormatter
{
    public string FormatNaturalLanguage<T>(
        T data,
        string metric,
        string? comparisonMetric = null,
        double? percentageChange = null)
    {
        var sb = new StringBuilder();

        if (data == null)
            return "No se encontraron datos para la consulta realizada.";

        try
        {
            var json = data is string s ? s : JsonSerializer.Serialize(data);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var metricValue = GetNumericValue(root, "total") ?? GetNumericValue(root, "formattedValue") ?? 0;
            var formattedValue = root.TryGetProperty("formattedValue", out var fv) ? fv.GetString() : null;
            var startDate = root.TryGetProperty("startDate", out var sd) ? sd.GetString() : null;
            var endDate = root.TryGetProperty("endDate", out var ed) ? ed.GetString() : null;

            sb.AppendLine(GenerateMetricDescription(metric, metricValue, formattedValue));

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sb.AppendLine($"Período: {FormatDateRange(startDate, endDate)}");
            }

            if (percentageChange.HasValue && comparisonMetric != null)
            {
                sb.AppendLine();
                sb.AppendLine(GenerateComparisonText(metric, percentageChange.Value, comparisonMetric));
            }

            if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                var topItems = items.EnumerateArray().Take(5).ToList();
                if (topItems.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Detalle:");
                    foreach (var item in topItems)
                    {
                        var label = item.TryGetProperty("label", out var l) ? l.GetString() : "Item";
                        var value = GetNumericValue(item, "value") ?? 0;
                        sb.AppendLine($"  - {label}: {FormatValue(metric, value)}");
                    }
                }
            }
        }
        catch
        {
            sb.AppendLine($"Los datos fueron retrieved pero no pudieron ser formateados completamente.");
        }

        return sb.ToString();
    }

    public string FormatError(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return "Ocurrió un error desconocido al procesar tu consulta.";

        if (errorMessage.Contains("conexión") || errorMessage.Contains("database"))
            return "No pude conectarme a la base de datos. Por favor intenta más tarde.";

        if (errorMessage.Contains("timeout"))
            return "La consulta tardó demasiado. Por favor intenta con un rango de fechas más pequeño.";

        if (errorMessage.Contains("no encontrado") || errorMessage.Contains("not found"))
            return "No encontré información para los criterios especificados.";

        return $"Error al procesar tu consulta: {errorMessage}";
    }

    public string FormatClarification(string question)
    {
        return $"Para ayudarte mejor, necesito que me aclares: {question}";
    }

    private string GenerateMetricDescription(string metric, double value, string? formattedValue)
    {
        var displayValue = formattedValue ?? FormatValue(metric, value);

        return metric switch
        {
            "sales" => string.Format("Las ventas totales {0} {1}", (value > 0 ? "fueron" : "no registran"), displayValue),
            "profit" => string.Format("La ganancia neta {0} {1}", (value > 0 ? "fue" : "no registra"), displayValue),
            "customers" => string.Format("Tienes {0} clientes registrados", displayValue),
            "products" => string.Format("Tienes {0} productos en tu catálogo", displayValue),
            "inventory" => string.Format("Tu inventario {0} {1}", (value > 0 ? "representa un valor de" : "no tiene valor registrado de"), displayValue),
            _ => string.Format("El total es {0}", displayValue)
        };
    }

    private string GenerateComparisonText(string metric, double percentageChange, string comparisonPeriod)
    {
        var direction = percentageChange >= 0 ? "incremento" : "disminución";
        var emoji = percentageChange >= 0 ? "📈" : "📉";
        var periodText = comparisonPeriod switch
        {
            "previous_week" => "la semana anterior",
            "previous_month" => "el mes anterior",
            "previous_year" => "el año anterior",
            _ => "el período anterior"
        };

        return $"{emoji} Hubo un {direction} del {Math.Abs(percentageChange):F1}% comparado con {periodText}.";
    }

    private string FormatDateRange(string start, string end)
    {
        if (start == end)
            return FormatSingleDate(start);

        try
        {
            var startDate = DateTime.Parse(start);
            var endDate = DateTime.Parse(end);

            if ((endDate - startDate).Days <= 1)
                return FormatSingleDate(start);

            return $"del {startDate:dd MMM} al {endDate:dd MMM yyyy}";
        }
        catch
        {
            return $"{start} a {end}";
        }
    }

    private string FormatSingleDate(string date)
    {
        try
        {
            var d = DateTime.Parse(date);
            var today = DateTime.UtcNow.Date;
            if (d.Date == today) return "hoy";
            if (d.Date == today.AddDays(-1)) return "ayer";
            return d.ToString("dd MMM yyyy");
        }
        catch
        {
            return date;
        }
    }

    private string FormatValue(string metric, double value)
    {
        if (metric == "sales" || metric == "profit" || metric == "inventory")
            return $"${value:N2} MXN";

        return value.ToString("N0");
    }

    private double? GetNumericValue(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return null;

        return prop.ValueKind switch
        {
            JsonValueKind.Number => prop.GetDouble(),
            JsonValueKind.String when double.TryParse(prop.GetString(), out var d) => d,
            _ => null
        };
    }
}
