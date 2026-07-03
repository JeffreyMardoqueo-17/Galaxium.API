using System.Text.Json.Serialization;

namespace Galaxium.Api.Services.AI.Models;

public class IntentResolution
{
    [JsonPropertyName("tool")]
    public string Tool { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public Dictionary<string, object?> Args { get; set; } = new();

    [JsonPropertyName("raw_text")]
    public string RawText { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("is_complete")]
    public bool IsComplete { get; set; }

    [JsonPropertyName("clarification_needed")]
    public string? ClarificationNeeded { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public static class IntentConstants
{
    public const string GetBusinessMetric = "GetBusinessMetric";
    public const string ExplainTrend = "ExplainTrend";
    public const string CompareMetrics = "CompareMetrics";

    public const string MetricSales = "sales";
    public const string MetricProfit = "profit";
    public const string MetricCustomers = "customers";
    public const string MetricProducts = "products";
    public const string MetricInventory = "inventory";

    public const string RangeToday = "today";
    public const string RangeYesterday = "yesterday";
    public const string RangeCurrentWeek = "current_week";
    public const string RangeLastWeek = "last_week";
    public const string RangeCurrentMonth = "current_month";
    public const string RangeLastMonth = "last_month";
    public const string RangeLast7Days = "last_7_days";
    public const string RangeLast30Days = "last_30_days";
    public const string RangeLast90Days = "last_90_days";
    public const string RangeCustom = "custom";

    public const string GroupByDay = "day";
    public const string GroupByWeek = "week";
    public const string GroupByMonth = "month";
    public const string GroupByCategory = "category";
    public const string GroupByProduct = "product";

    public const string ComparisonPreviousWeek = "previous_week";
    public const string ComparisonPreviousMonth = "previous_month";
    public const string ComparisonPreviousYear = "previous_year";
}
