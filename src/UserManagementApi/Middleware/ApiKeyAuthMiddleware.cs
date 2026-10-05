using System.Security.Cryptography;
using System.Text;

namespace UserManagementApi.Middleware;

/// <summary>
/// Simple API-key authentication. Clients must send the key in the X-API-KEY header.
/// Swagger UI and /health are left open.
/// </summary>
public class ApiKeyAuthMiddleware
{
    public const string HeaderName = "X-API-KEY";
    private static readonly string[] PublicPrefixes = { "/swagger", "/health" };

    private readonly RequestDelegate _next;
    private readonly string _apiKey;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration config, ILogger<ApiKeyAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _apiKey = config["Authentication:ApiKey"]
                  ?? throw new InvalidOperationException("Authentication:ApiKey is not configured.");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (PublicPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided))
        {
            await RejectAsync(context, $"Missing {HeaderName} header.");
            return;
        }

        if (!KeysMatch(provided.ToString(), _apiKey))
        {
            await RejectAsync(context, "Invalid API key.");
            return;
        }

        await _next(context);
    }

    private static bool KeysMatch(string provided, string expected)
    {
        var a = SHA256.HashData(Encoding.UTF8.GetBytes(provided));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(a, b); // constant-time compare
    }

    private async Task RejectAsync(HttpContext context, string message)
    {
        _logger.LogWarning("Unauthorized request to {Path}: {Reason}", context.Request.Path, message);
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { status = 401, title = "Unauthorized", detail = message });
    }
}
