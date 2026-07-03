using Galaxium.Api.Services.AI.Models;

namespace Galaxium.Api.Services.AI.Interfaces;

public interface IConversationContextStore
{
    Task<ConversationState?> GetContextAsync(
        string tenantId,
        string userId,
        string conversationId,
        CancellationToken cancellationToken = default);

    Task SaveContextAsync(
        string tenantId,
        string userId,
        string conversationId,
        ConversationState state,
        CancellationToken cancellationToken = default);

    Task ClearContextAsync(
        string tenantId,
        string userId,
        string conversationId,
        CancellationToken cancellationToken = default);

    string BuildKey(string tenantId, string userId, string conversationId);
}
