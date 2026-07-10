using Galaxium.Api.DTOs.AI;

namespace Galaxium.Api.Services.AI.Interfaces;

public interface IAICopilotService
{
    Task<AIChatResponseDto> ProcessMessageAsync(
        AIChatRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ConversationContextDto?> GetConversationContextAsync(
        int tenantId,
        int userId,
        string conversationId,
        CancellationToken cancellationToken = default);

    Task ClearConversationAsync(
        int tenantId,
        int userId,
        string conversationId,
        CancellationToken cancellationToken = default);
}
