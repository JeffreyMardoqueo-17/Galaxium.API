using Galaxium.Api.DTOs.AI;
using Galaxium.Api.Services.AI.Interfaces;
using Galaxium.Api.Services.AI.Models;
using Microsoft.Extensions.Logging;

namespace Galaxium.Api.Services.AI.Core;

public class AICopilotService : IAICopilotService
{
    private readonly IIntentParser _intentParser;
    private readonly IToolExecutor _toolExecutor;
    private readonly IToolRegistry _toolRegistry;
    private readonly IConversationContextStore _contextStore;
    private readonly IResponseFormatter _responseFormatter;
    private readonly ILogger<AICopilotService> _logger;

    public AICopilotService(
        IIntentParser intentParser,
        IToolExecutor toolExecutor,
        IToolRegistry toolRegistry,
        IConversationContextStore contextStore,
        IResponseFormatter responseFormatter,
        ILogger<AICopilotService> logger)
    {
        _intentParser = intentParser;
        _toolExecutor = toolExecutor;
        _toolRegistry = toolRegistry;
        _contextStore = contextStore;
        _responseFormatter = responseFormatter;
        _logger = logger;
    }

    public async Task<AIChatResponseDto> ProcessMessageAsync(
        AIChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = new AIChatResponseDto
        {
            ConversationId = request.ConversationId ?? Guid.NewGuid().ToString(),
            Success = true
        };

        try
        {
            _logger.LogInformation("Processing message from User:{UserId}, Tenant:{TenantId}",
                request.UserId, request.TenantId);

            var conversationId = response.ConversationId;
            var previousContext = await _contextStore.GetContextAsync(
                request.TenantId.ToString(),
                request.UserId.ToString(),
                conversationId,
                cancellationToken);

            var toolsSchema = _toolRegistry.GetToolsDescription();

            var intent = await _intentParser.ParseAsync(
                request.Message,
                previousContext,
                toolsSchema,
                cancellationToken);

            if (!string.IsNullOrEmpty(intent.Error))
            {
                response.Success = false;
                response.Error = intent.Error;
                response.Response = _responseFormatter.FormatError(intent.Error);
                return response;
            }

            if (!intent.IsComplete || string.IsNullOrEmpty(intent.Tool))
            {
                response.RequiresClarification = true;
                response.ClarificationPrompt = intent.ClarificationNeeded ?? "¿Podrías ser más específico?";
                response.Response = _responseFormatter.FormatClarification(response.ClarificationPrompt);
                return response;
            }

            var toolResult = await _toolExecutor.ExecuteAsync(
                intent.Tool,
                intent.Args,
                request.TenantId,
                request.UserId,
                cancellationToken);

            if (!toolResult.Success)
            {
                response.Success = false;
                response.Error = toolResult.Error;
                response.Response = _responseFormatter.FormatError(toolResult.Error ?? "Error desconocido");
                return response;
            }

            var metric = intent.Args.GetValueOrDefault("metric")?.ToString() ?? "unknown";
            var comparison = intent.Args.GetValueOrDefault("comparison")?.ToString();
            double? percentageChange = null;

            if (toolResult.Data != null)
            {
                try
                {
                    var dataJson = System.Text.Json.JsonSerializer.Serialize(toolResult.Data);
                    using var doc = System.Text.Json.JsonDocument.Parse(dataJson);
                    if (doc.RootElement.TryGetProperty("comparison", out var compElement))
                    {
                        if (compElement.TryGetProperty("percentageChange", out var pc))
                        {
                            percentageChange = pc.GetDouble();
                        }
                    }
                }
                catch { }
            }

            response.Data = toolResult.Data;
            response.Response = _responseFormatter.FormatNaturalLanguage(
                toolResult.Data,
                metric,
                comparison,
                percentageChange);

            if (percentageChange.HasValue)
            {
                response.MetricSummary = new MetricSummaryDto
                {
                    Metric = metric,
                    FormattedValue = toolResult.FormattedValue,
                    NumericValue = toolResult.NumericValue,
                    ChangeDirection = percentageChange >= 0 ? "up" : "down",
                    PercentageChange = percentageChange,
                    ComparisonPeriod = comparison
                };
            }

            await SaveConversationContextAsync(
                request.TenantId.ToString(),
                request.UserId.ToString(),
                conversationId,
                intent,
                toolResult,
                request.Message,
                cancellationToken);

            _logger.LogInformation("Successfully processed message. Tool:{Tool}, Metric:{Metric}",
                intent.Tool, metric);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI chat message");
            response.Success = false;
            response.Error = ex.Message;
            response.Response = _responseFormatter.FormatError(ex.Message);
            return response;
        }
    }

    public async Task<ConversationContextDto?> GetConversationContextAsync(
        int tenantId,
        int userId,
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var context = await _contextStore.GetContextAsync(
            tenantId.ToString(),
            userId.ToString(),
            conversationId,
            cancellationToken);

        if (context == null)
            return null;

        return new ConversationContextDto
        {
            ConversationId = conversationId,
            LastMetric = context.LastMetric,
            LastRangeType = context.LastRangeType,
            LastRangeStart = context.LastRangeStart,
            LastRangeEnd = context.LastRangeEnd,
            LastGroupBy = context.LastGroupBy,
            LastComparison = context.LastComparison,
            LastUpdated = context.LastUpdated,
            MessageCount = context.History.Count
        };
    }

    public async Task ClearConversationAsync(
        int tenantId,
        int userId,
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        await _contextStore.ClearContextAsync(
            tenantId.ToString(),
            userId.ToString(),
            conversationId,
            cancellationToken);
    }

    private async Task SaveConversationContextAsync(
        string tenantId,
        string userId,
        string conversationId,
        IntentResolution intent,
        ToolExecutionResult result,
        string userMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = await _contextStore.GetContextAsync(
                tenantId, userId, conversationId, cancellationToken)
                ?? new ConversationState();

            context.LastMetric = intent.Args.GetValueOrDefault("metric")?.ToString();
            context.LastRangeType = intent.Args.GetValueOrDefault("rangeType")?.ToString();
            context.LastGroupBy = intent.Args.GetValueOrDefault("groupBy")?.ToString();
            context.LastComparison = intent.Args.GetValueOrDefault("comparison")?.ToString();

            if (intent.Args.TryGetValue("startDate", out var startDate) && startDate != null)
            {
                if (DateTime.TryParse(startDate.ToString(), out var sd))
                    context.LastRangeStart = sd;
            }

            if (intent.Args.TryGetValue("endDate", out var endDate) && endDate != null)
            {
                if (DateTime.TryParse(endDate.ToString(), out var ed))
                    context.LastRangeEnd = ed;
            }

            context.LastResult = result.Data;
            context.NumericValue = result.NumericValue;
            context.LastUpdated = DateTime.UtcNow;

            context.History.Add(new ChatMessage
            {
                Role = "user",
                Content = userMessage,
                Timestamp = DateTime.UtcNow
            });

            context.History.Add(new ChatMessage
            {
                Role = "assistant",
                Content = result.FormattedValue ?? result.Data?.ToString() ?? "Consulta procesada",
                Timestamp = DateTime.UtcNow
            });

            if (context.History.Count > 20)
            {
                context.History = context.History.TakeLast(20).ToList();
            }

            await _contextStore.SaveContextAsync(tenantId, userId, conversationId, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save conversation context");
        }
    }
}
