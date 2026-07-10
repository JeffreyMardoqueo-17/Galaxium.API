using Galaxium.Api.Services.AI.Models;

namespace Galaxium.Api.Services.AI.Interfaces;

public interface IToolRegistry
{
    void RegisterTool(ToolDefinition tool);
    void RegisterTools(IEnumerable<ToolDefinition> tools);
    ToolDefinition? GetTool(string toolName);
    IReadOnlyList<ToolDefinition> GetAllTools();
    string GetToolsSchema();
    string GetToolsDescription();
    bool HasTool(string toolName);
}
