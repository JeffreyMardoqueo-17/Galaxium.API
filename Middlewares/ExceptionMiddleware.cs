using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Galaxium.API.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Galaxium.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // ==========================
            // Request context
            // ==========================
            var request = context.Request;
            var endpoint = $"{request.Method} {request.Path}";
            var queryString = request.QueryString.ToString();
            var traceId = context.TraceIdentifier;

            // ==========================
            // User context (if authenticated)
            // ==========================
            var userId = context.User?.Claims?
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var userName = context.User?.Identity?.Name;

            // ==========================
            // Logging (full context)
            // ==========================
            _logger.LogError(ex,
                "Unhandled exception occurred | " +
                "Endpoint: {Endpoint} | Query: {Query} | UserId: {UserId} | UserName: {UserName} | TraceId: {TraceId}",
                endpoint,
                queryString,
                userId ?? "Anonymous",
                userName ?? "Anonymous",
                traceId
            );

            // ==========================
            // HTTP response
            // ==========================
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetStatusCode(ex);

            var response = new ErrorResponse
            {
                StatusCode = context.Response.StatusCode,
                Message = GetClientMessage(ex),
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }

        // ==========================
        // Status code mapping
        // ==========================
        private static int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                BusinessException be => be.StatusCode,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        // ==========================
        // Client-safe messages
        // ==========================
        private static string GetClientMessage(Exception ex)
        {
            return ex switch
            {
                BusinessException => ex.Message,
                KeyNotFoundException => "The requested resource was not found.",
                UnauthorizedAccessException => "Unauthorized access.",
                ArgumentException => ex.Message,
                _ => "An unexpected error occurred on the server."
            };
        }

        // ==========================
        // Response DTO
        // ==========================
        private sealed class ErrorResponse
        {
            public int StatusCode { get; init; }
            public string Message { get; init; } = string.Empty;
            public string TraceId { get; init; } = string.Empty;
            public DateTime Timestamp { get; init; }
        }
    }
}
