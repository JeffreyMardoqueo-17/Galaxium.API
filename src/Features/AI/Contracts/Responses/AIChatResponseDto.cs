namespace Galaxium.Api.DTOs.AI;

public class AIChatResponseDto
{
    public string ConversationId { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public bool RequiresClarification { get; set; }
    public string? ClarificationPrompt { get; set; }
    public object? Data { get; set; }
    public MetricSummaryDto? MetricSummary { get; set; }
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}

public class MetricSummaryDto
{
    public string Metric { get; set; } = string.Empty;
    public string? FormattedValue { get; set; }
    public double? NumericValue { get; set; }
    public string? ChangeDirection { get; set; }
    public double? PercentageChange { get; set; }
    public string? ComparisonPeriod { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
