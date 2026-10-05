using System.Diagnostics;

namespace UserManagementApi.Middleware;

/// <summary>Logs every request with method, path, status code, duration and a correlation id.</summary>
public class RequestLoggingMiddleware
{
    public const string CorrelationHeader = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationHeader, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString("N");

        context.Response.Headers[CorrelationHeader] = correlationId;

        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        _logger.LogInformation("--> {Method} {Path}", context.Request.Method, context.Request.Path);
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("<-- {Method} {Path} responded {StatusCode} in {Elapsed} ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
