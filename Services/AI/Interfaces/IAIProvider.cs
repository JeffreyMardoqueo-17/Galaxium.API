using Galaxium.Api.Services.AI.Models;

namespace Galaxium.Api.Services.AI.Interfaces;

public interface IAIProvider
{
    string ProviderName { get; }
    string Model { get; }
    Task<string> GenerateContentAsync(
        string systemInstruction,
        string userMessage,
        CancellationToken cancellationToken = default);
    Task<string> GenerateContentAsync(
        string prompt,
        CancellationToken cancellationToken = default);
    Task<FunctionCallResult> GenerateContentWithToolsAsync(
        string systemInstruction,
        string userMessage,
        IEnumerable<ToolDefinition> tools,
        CancellationToken cancellationToken = default);
}

public class FunctionCallResult
{
    public bool HasFunctionCall { get; set; }
    public string? FunctionName { get; set; }
    public Dictionary<string, object?>? Arguments { get; set; }
    public string? TextResponse { get; set; }
    public bool IsQuotaExceeded { get; set; }
}
