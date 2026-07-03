using System.Text.Json.Serialization;

namespace Galaxium.Api.Services.AI.Models;

public class ConversationState
{
    public string? LastMetric { get; set; }
    public string? LastRangeType { get; set; }
    public DateTime? LastRangeStart { get; set; }
    public DateTime? LastRangeEnd { get; set; }
    public string? LastGroupBy { get; set; }
    public string? LastComparison { get; set; }
    public Dictionary<string, object> LastFilters { get; set; } = new();
    public object? LastResult { get; set; }
    public double? LastTotalValue { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> History { get; set; } = new();
    public bool IsComparisonActive { get; set; }
    public object? PreviousPeriodResult { get; set; }
    public double? NumericValue { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
