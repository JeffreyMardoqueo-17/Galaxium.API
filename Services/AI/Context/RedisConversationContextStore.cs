using System.Text.Json;
using Galaxium.Api.Services.AI.Interfaces;
using Galaxium.Api.Services.AI.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Galaxium.Api.Services.AI.Context;

public class RedisConversationContextStore : IConversationContextStore
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisConversationContextStore> _logger;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(30);
    private const string KeyPrefix = "ai:context";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisConversationContextStore(
        IDistributedCache cache,
        ILogger<RedisConversationContextStore> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<ConversationState?> GetContextAsync(
        string tenantId,
        string userId,
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = BuildKey(tenantId, userId, conversationId);
            var json = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(json))
            {
                _logger.LogDebug("No context found for key: {Key}", key);
                return null;
            }

            var state = JsonSerializer.Deserialize<ConversationState>(json, JsonOptions);
            _logger.LogDebug("Context retrieved for key: {Key}", key);
            return state;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving context for tenant: {TenantId}, user: {UserId}, conversation: {ConversationId}",
                tenantId, userId, conversationId);
            return null;
        }
    }

    public async Task SaveContextAsync(
        string tenantId,
        string userId,
        string conversationId,
        ConversationState state,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = BuildKey(tenantId, userId, conversationId);
            state.LastUpdated = DateTime.UtcNow;

            var json = JsonSerializer.Serialize(state, JsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DefaultTtl,
                SlidingExpiration = DefaultTtl
            };

            await _cache.SetStringAsync(key, json, options, cancellationToken);
            _logger.LogDebug("Context saved for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving context for tenant: {TenantId}, user: {UserId}, conversation: {ConversationId}",
                tenantId, userId, conversationId);
            throw;
        }
    }

    public async Task ClearContextAsync(
        string tenantId,
        string userId,
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = BuildKey(tenantId, userId, conversationId);
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Context cleared for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing context for tenant: {TenantId}, user: {UserId}, conversation: {ConversationId}",
                tenantId, userId, conversationId);
            throw;
        }
    }

    public string BuildKey(string tenantId, string userId, string conversationId)
    {
        return $"{KeyPrefix}:{tenantId}:{userId}:{conversationId}";
    }
}
