namespace Galaxium.Api.DTOs.AI;

public class ToolResultDto
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Error { get; set; }
    public long ExecutionTimeMs { get; set; }
}
