namespace Galaxium.Api.DTOs.AI;

public class ToolCallDto
{
    public string Tool { get; set; } = string.Empty;
    public Dictionary<string, object?> Args { get; set; } = new();
}
