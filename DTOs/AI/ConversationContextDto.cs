namespace Galaxium.Api.DTOs.AI;

public class ConversationContextDto
{
    public string ConversationId { get; set; } = string.Empty;
    public string? LastMetric { get; set; }
    public string? LastRangeType { get; set; }
    public DateTime? LastRangeStart { get; set; }
    public DateTime? LastRangeEnd { get; set; }
    public string? LastGroupBy { get; set; }
    public string? LastComparison { get; set; }
    public DateTime LastUpdated { get; set; }
    public int MessageCount { get; set; }
}
