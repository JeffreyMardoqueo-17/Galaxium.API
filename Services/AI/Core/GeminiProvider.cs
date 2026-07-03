using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Galaxium.Api.Services.AI.Interfaces;
using Galaxium.Api.Services.AI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Galaxium.Api.Services.AI.Core;

public class GeminiProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiProvider> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 1000;

    public string ProviderName => "Google Gemini";
    public string Model => _model;

    public GeminiProvider(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["AI:GeminiApiKey"] ?? throw new InvalidOperationException("GEMINI_API_KEY no está configurado");
        _model = configuration["AI:GeminiModel"] ?? "gemini-2.0-flash";
    }

    public async Task<string> GenerateContentAsync(
        string systemInstruction,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"{systemInstruction}\n\n---\n\nUsuario: {userMessage}\n\nAsistente:";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                topK = 20,
                topP = 0.8,
                maxOutputTokens = 1024
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

        int retryCount = 0;
        int delay = InitialDelayMs;

        while (retryCount < MaxRetries)
        {
            try
            {
                _logger.LogDebug("Sending request to Gemini API (attempt {Attempt})", retryCount + 1);

                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return ParseResponse(responseJson);
                }

                if ((int)response.StatusCode == 429)
                {
                    retryCount++;
                    if (retryCount >= MaxRetries)
                    {
                        _logger.LogWarning("Gemini API rate limit exceeded after {MaxRetries} retries", MaxRetries);
                        throw new GeminiQuotaExceededException(
                            "Has excedido el límite de solicitudes a Gemini. " +
                            "Espera unos segundos e intenta de nuevo. " +
                            "El tier gratuito tiene límites muy restrictivos (15 requests/min).");
                    }

                    _logger.LogWarning("Gemini API rate limited, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})", 
                        delay, retryCount, MaxRetries);
                    
                    await Task.Delay(delay, cancellationToken);
                    delay *= 2;
                    continue;
                }

                var errorMessage = ParseErrorMessage(responseJson);
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorMessage);
                throw new Exception($"Error de Gemini: {errorMessage}");
            }
            catch (GeminiQuotaExceededException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                retryCount++;
                if (retryCount >= MaxRetries)
                {
                    _logger.LogError(ex, "Network error after {MaxRetries} retries", MaxRetries);
                    throw new Exception("Error de conexión con Gemini. Verifica tu conexión a internet.");
                }
                
                _logger.LogWarning(ex, "Network error, retrying in {Delay}ms", delay);
                await Task.Delay(delay, cancellationToken);
                delay *= 2;
            }
        }

        throw new Exception("Error al procesar la solicitud después de múltiples intentos.");
    }

    public async Task<FunctionCallResult> GenerateContentWithToolsAsync(
        string systemInstruction,
        string userMessage,
        IEnumerable<ToolDefinition> tools,
        CancellationToken cancellationToken = default)
    {
        var functionDeclarations = tools.Select(t => new
        {
            name = t.Name,
            description = t.Description,
            parameters = new
            {
                type = "object",
                properties = t.Parameters.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        type = kvp.Value.Type,
                        description = kvp.Value.Description
                    }),
                required = t.Parameters.Where(p => p.Value.Required).Select(p => p.Key).ToList()
            }
        }).ToList();

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = $"{systemInstruction}\n\nUsuario: {userMessage}" }
                    }
                }
            },
            tools = new[]
            {
                new { function_declarations = functionDeclarations }
            },
            generationConfig = new
            {
                temperature = 0.1,
                topK = 20,
                topP = 0.8,
                maxOutputTokens = 1024
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

        int retryCount = 0;
        int delay = InitialDelayMs;

        while (retryCount < MaxRetries)
        {
            try
            {
                _logger.LogDebug("Sending function calling request to Gemini API (attempt {Attempt})", retryCount + 1);

                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return ParseFunctionCallResponse(responseJson);
                }

                if ((int)response.StatusCode == 429)
                {
                    retryCount++;
                    if (retryCount >= MaxRetries)
                    {
                        _logger.LogWarning("Gemini API rate limit exceeded after {MaxRetries} retries", MaxRetries);
                        return new FunctionCallResult
                        {
                            IsQuotaExceeded = true,
                            HasFunctionCall = false,
                            TextResponse = "Has excedido el límite de solicitudes a Gemini. Espera unos segundos e intenta de nuevo."
                        };
                    }

                    _logger.LogWarning("Gemini API rate limited, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})", 
                        delay, retryCount, MaxRetries);
                    
                    await Task.Delay(delay, cancellationToken);
                    delay *= 2;
                    continue;
                }

                var errorMessage = ParseErrorMessage(responseJson);
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorMessage);
                return new FunctionCallResult
                {
                    HasFunctionCall = false,
                    TextResponse = $"Error de Gemini: {errorMessage}"
                };
            }
            catch (HttpRequestException ex)
            {
                retryCount++;
                if (retryCount >= MaxRetries)
                {
                    _logger.LogError(ex, "Network error after {MaxRetries} retries", MaxRetries);
                    return new FunctionCallResult
                    {
                        HasFunctionCall = false,
                        TextResponse = "Error de conexión con Gemini. Verifica tu conexión a internet."
                    };
                }
                
                _logger.LogWarning(ex, "Network error, retrying in {Delay}ms", delay);
                await Task.Delay(delay, cancellationToken);
                delay *= 2;
            }
        }

        return new FunctionCallResult
        {
            HasFunctionCall = false,
            TextResponse = "Error al procesar la solicitud después de múltiples intentos."
        };
    }

    private FunctionCallResult ParseFunctionCallResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && 
                candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                
                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var part = parts[0];

                    if (part.TryGetProperty("functionCall", out var functionCall))
                    {
                        var functionName = functionCall.GetProperty("name").GetString();
                        var args = new Dictionary<string, object?>();
                        
                        if (functionCall.TryGetProperty("args", out var argsElement))
                        {
                            foreach (var prop in argsElement.EnumerateObject())
                            {
                                args[prop.Name] = ParseJsonValue(prop.Value);
                            }
                        }

                        _logger.LogDebug("Gemini function call: {FunctionName}", functionName);
                        return new FunctionCallResult
                        {
                            HasFunctionCall = true,
                            FunctionName = functionName,
                            Arguments = args
                        };
                    }

                    if (part.TryGetProperty("text", out var textElement))
                    {
                        var text = textElement.GetString();
                        _logger.LogDebug("Gemini text response: {Length} characters", text?.Length ?? 0);
                        return new FunctionCallResult
                        {
                            HasFunctionCall = false,
                            TextResponse = text ?? string.Empty
                        };
                    }
                }
            }

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var errorMsg = error.GetProperty("message").GetString();
                return new FunctionCallResult
                {
                    HasFunctionCall = false,
                    TextResponse = $"Error de Gemini: {errorMsg}"
                };
            }

            return new FunctionCallResult
            {
                HasFunctionCall = false,
                TextResponse = "Respuesta de Gemini no reconocida"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response: {Response}", responseJson);
            return new FunctionCallResult
            {
                HasFunctionCall = false,
                TextResponse = "Error al procesar la respuesta de Gemini."
            };
        }
    }

    private object? ParseJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private string ParseResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && 
                candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                
                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString();
                    _logger.LogDebug("Gemini response received: {Length} characters", text?.Length ?? 0);
                    return text ?? string.Empty;
                }
            }

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var errorMsg = error.GetProperty("message").GetString();
                throw new Exception($"Error de Gemini: {errorMsg}");
            }

            throw new Exception("Respuesta de Gemini no reconocida");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response: {Response}", responseJson);
            throw new Exception("Error al procesar la respuesta de Gemini.");
        }
    }

    private string ParseErrorMessage(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                if (error.TryGetProperty("message", out var message))
                {
                    return message.GetString() ?? "Error desconocido";
                }
            }

            if (doc.RootElement.TryGetProperty("error", out var errorObj) && 
                errorObj.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in errorObj.EnumerateObject())
                {
                    if (prop.Name.Contains("quota", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Has excedido el límite de cuota de Gemini. Espera e intenta de nuevo.";
                    }
                }
            }
        }
        catch { }

        return "Error desconocido de Gemini";
    }

    public async Task<string> GenerateContentAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        return await GenerateContentAsync(string.Empty, prompt, cancellationToken);
    }
}

public class GeminiQuotaExceededException : Exception
{
    public GeminiQuotaExceededException(string message) : base(message) { }
    public GeminiQuotaExceededException(string message, Exception inner) : base(message, inner) { }
}

public class AIClientFactory
{
    private readonly HttpClient _httpClient;

    public AIClientFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public IAIProvider CreateProvider(string providerType, IConfiguration configuration, ILogger<GeminiProvider> logger)
    {
        return providerType.ToLower() switch
        {
            "gemini" => new GeminiProvider(_httpClient, configuration, logger),
            _ => throw new NotSupportedException($"Provider '{providerType}' no está soportado")
        };
    }
}
