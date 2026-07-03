using Galaxium.Api.Services.AI.Models;

namespace Galaxium.Api.Services.AI.Interfaces;

public interface IIntentParser
{
    Task<IntentResolution> ParseAsync(
        string userMessage,
        ConversationState? previousContext,
        string toolsSchema,
        CancellationToken cancellationToken = default);
}
