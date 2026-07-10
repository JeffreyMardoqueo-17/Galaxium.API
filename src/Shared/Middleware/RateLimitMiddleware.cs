using System.Collections.Concurrent;
using System.Security.Claims;

namespace Galaxium.API.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        // Almacena requests por usuario
        private static readonly ConcurrentDictionary<string, List<DateTime>> _requests
            = new();

        private const int LIMIT_AUTH = 30;
        private const int LIMIT_ANON = 10;
        private static readonly TimeSpan WINDOW = TimeSpan.FromMinutes(1);

        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var key = GetClientKey(context);
            var now = DateTime.UtcNow;

            var requestTimes = _requests.GetOrAdd(key, _ => new List<DateTime>());

            bool isRateLimited = false;

            lock (requestTimes)
            {
                // Limpiar requests viejos
                requestTimes.RemoveAll(t => now - t > WINDOW);

                var limit = IsAuthenticated(context)
                    ? LIMIT_AUTH
                    : LIMIT_ANON;

                if (requestTimes.Count >= limit)
                {
                    isRateLimited = true;
                }
                else
                {
                    requestTimes.Add(now);
                }
            }

            if (isRateLimited)
            {
                context.Response.StatusCode = 429;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    StatusCode = 429,
                    Message = "Too many requests. Please try again later."
                };

                await context.Response.WriteAsJsonAsync(response);
                return;
            }

            await _next(context);
        }

        // ==========================
        // Helpers
        // ==========================

        private static string GetClientKey(HttpContext context)
        {
            if (IsAuthenticated(context))
            {
                return context.User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value ?? "unknown-user";
            }

            return context.Connection.RemoteIpAddress?.ToString()
                   ?? "unknown-ip";
        }

        private static bool IsAuthenticated(HttpContext context)
        {
            return context.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
