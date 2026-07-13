using System.Security.Claims;

namespace Galaxium.Api.Shared.MultiTenant;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var tenantIdClaim = context.User?.FindFirst("tenant_id")?.Value;

            if (int.TryParse(tenantIdClaim, out var tenantId) && tenantId > 0)
            {
                TenantContext.SetTenantId(tenantId);
            }
            else if (IsPublicEndpoint(context))
            {
                await _next(context);
                return;
            }

            await _next(context);
        }
        finally
        {
            TenantContext.Clear();
        }
    }

    private static bool IsPublicEndpoint(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        return path.Contains("/api/User/first-register", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/User/login", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/User/forgot-password", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/User/reset-password", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/health", StringComparison.OrdinalIgnoreCase);
    }
}
