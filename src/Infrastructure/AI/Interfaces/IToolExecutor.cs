using Galaxium.Api.Services.AI.Models;

namespace Galaxium.Api.Services.AI.Interfaces;

public interface IToolExecutor
{
    Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        Dictionary<string, object> arguments,
        int tenantId,
        int userId,
        CancellationToken cancellationToken = default);

    bool CanExecute(string toolName);
}
