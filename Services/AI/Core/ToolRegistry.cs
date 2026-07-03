using System.Text;
using System.Text.Json;
using Galaxium.Api.Services.AI.Interfaces;
using Galaxium.Api.Services.AI.Models;

namespace Galaxium.Api.Services.AI.Core;

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ToolDefinition> _tools = new();
    private readonly object _lock = new();

    public ToolRegistry()
    {
        RegisterDefaultTools();
    }

    private void RegisterDefaultTools()
    {
        RegisterTool(new ToolDefinition
        {
            Name = IntentConstants.GetBusinessMetric,
            Description = "Obtiene métricas de negocio como ventas, ganancias, clientes, productos e inventario. SIEMPRE usa esta herramienta para consultar datos.",
            Parameters = new Dictionary<string, ToolParameter>
            {
                ["metric"] = new ToolParameter
                {
                    Type = "string",
                    Description = "Tipo de métrica: sales, profit, customers, products, inventory",
                    Required = true,
                    EnumValues = new List<string>
                    {
                        IntentConstants.MetricSales,
                        IntentConstants.MetricProfit,
                        IntentConstants.MetricCustomers,
                        IntentConstants.MetricProducts,
                        IntentConstants.MetricInventory
                    }
                },
                ["rangeType"] = new ToolParameter
                {
                    Type = "string",
                    Description = "Tipo de rango temporal: today, yesterday, current_week, last_week, current_month, last_month, last_7_days, last_30_days, last_90_days, custom",
                    Required = true,
                    EnumValues = new List<string>
                    {
                        IntentConstants.RangeToday,
                        IntentConstants.RangeYesterday,
                        IntentConstants.RangeCurrentWeek,
                        IntentConstants.RangeLastWeek,
                        IntentConstants.RangeCurrentMonth,
                        IntentConstants.RangeLastMonth,
                        IntentConstants.RangeLast7Days,
                        IntentConstants.RangeLast30Days,
                        IntentConstants.RangeLast90Days,
                        IntentConstants.RangeCustom
                    },
                    DefaultValue = IntentConstants.RangeCurrentMonth
                },
                ["startDate"] = new ToolParameter
                {
                    Type = "string",
                    Description = "Fecha de inicio (YYYY-MM-DD) - solo para rangeType=custom",
                    Required = false
                },
                ["endDate"] = new ToolParameter
                {
                    Type = "string",
                    Description = "Fecha de fin (YYYY-MM-DD) - solo para rangeType=custom",
                    Required = false
                },
                ["groupBy"] = new ToolParameter
                {
                    Type = "string",
                    Description = "Agrupación: day, week, month, category, product",
                    Required = false,
                    EnumValues = new List<string>
                    {
                        IntentConstants.GroupByDay,
                        IntentConstants.GroupByWeek,
                        IntentConstants.GroupByMonth,
                        IntentConstants.GroupByCategory,
                        IntentConstants.GroupByProduct
                    },
                    DefaultValue = IntentConstants.GroupByDay
                },
                ["comparison"] = new ToolParameter
                {
                    Type = "string",
                    Description = "Comparación con periodo anterior: previous_week, previous_month, previous_year",
                    Required = false,
                    EnumValues = new List<string>
                    {
                        IntentConstants.ComparisonPreviousWeek,
                        IntentConstants.ComparisonPreviousMonth,
                        IntentConstants.ComparisonPreviousYear
                    }
                },
                ["categoryId"] = new ToolParameter
                {
                    Type = "integer",
                    Description = "Filtrar por categoría de producto",
                    Required = false
                },
                ["customerId"] = new ToolParameter
                {
                    Type = "integer",
                    Description = "Filtrar por cliente específico",
                    Required = false
                },
                ["limit"] = new ToolParameter
                {
                    Type = "integer",
                    Description = "Límite de resultados (para top productos, etc.)",
                    Required = false,
                    DefaultValue = 10
                }
            }
        });

        RegisterTool(new ToolDefinition
        {
            Name = IntentConstants.ExplainTrend,
            Description = "Explica por qué hubo un cambio en una métrica. Usa los datos del último resultado de GetBusinessMetric.",
            Parameters = new Dictionary<string, ToolParameter>
            {
                ["metric"] = new ToolParameter
                {
                    Type = "string",
                    Description = "La métrica a explicar",
                    Required = true
                },
                ["changePercentage"] = new ToolParameter
                {
                    Type = "number",
                    Description = "Porcentaje de cambio (positivo o negativo)",
                    Required = true
                },
                ["direction"] = new ToolParameter
                {
                    Type = "string",
                    Description = "Dirección del cambio: up, down",
                    Required = true
                }
            }
        });
    }

    public void RegisterTool(ToolDefinition tool)
    {
        lock (_lock)
        {
            _tools[tool.Name] = tool;
        }
    }

    public void RegisterTools(IEnumerable<ToolDefinition> tools)
    {
        foreach (var tool in tools)
        {
            RegisterTool(tool);
        }
    }

    public ToolDefinition? GetTool(string toolName)
    {
        lock (_lock)
        {
            return _tools.TryGetValue(toolName, out var tool) ? tool : null;
        }
    }

    public IReadOnlyList<ToolDefinition> GetAllTools()
    {
        lock (_lock)
        {
            return _tools.Values.ToList().AsReadOnly();
        }
    }

    public bool HasTool(string toolName)
    {
        lock (_lock)
        {
            return _tools.ContainsKey(toolName);
        }
    }

    public string GetToolsSchema()
    {
        var tools = GetAllTools();
        return JsonSerializer.Serialize(tools, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    public string GetToolsDescription()
    {
        var sb = new StringBuilder();
        sb.AppendLine("HERRAMIENTAS DISPONIBLES:");
        sb.AppendLine();

        foreach (var tool in GetAllTools())
        {
            sb.AppendLine($"## {tool.Name}");
            sb.AppendLine($"{tool.Description}");
            sb.AppendLine();
            sb.AppendLine("Parámetros:");
            foreach (var (paramName, param) in tool.Parameters)
            {
                var required = param.Required ? "(OBLIGATORIO)" : "(opcional)";
                var enumInfo = param.EnumValues != null ? $" [Valores: {string.Join(", ", param.EnumValues)}]" : "";
                var defaultInfo = param.DefaultValue != null ? $" Por defecto: {param.DefaultValue}" : "";
                sb.AppendLine($"  - {paramName}: {param.Description} {required}{enumInfo}{defaultInfo}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
