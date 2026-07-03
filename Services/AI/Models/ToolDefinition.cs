using System.Text.Json.Serialization;

namespace Galaxium.Api.Services.AI.Models;

public class ToolDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public Dictionary<string, ToolParameter> Parameters { get; set; } = new();
}

public class ToolParameter
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    [JsonPropertyName("enum")]
    public List<string>? EnumValues { get; set; }

    [JsonPropertyName("default")]
    public object? DefaultValue { get; set; }
}

public class ToolExecutionResult
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Error { get; set; }
    public long ExecutionTimeMs { get; set; }
    public string? FormattedValue { get; set; }
    public double? NumericValue { get; set; }
}

public class MetricData
{
    public string Metric { get; set; } = string.Empty;
    public double Total { get; set; }
    public double? ComparisonTotal { get; set; }
    public double? PercentageChange { get; set; }
    public string? ChangeDirection { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? GroupBy { get; set; }
    public List<MetricItem> Items { get; set; } = new();
    public double? NumericValue { get; set; }
    public string? FormattedValue { get; set; }
}

public class MetricItem
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public int? Count { get; set; }
}
