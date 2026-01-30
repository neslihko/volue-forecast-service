using System.Diagnostics;

namespace Volue.ForecastService.Api.Middleware;

/// <summary>
/// Middleware to extract or generate correlation IDs for distributed tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdKey = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request header
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

        // Generate new one if not provided
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Store in HttpContext for later retrieval
        context.Items[CorrelationIdKey] = correlationId;

        // Add to activity for distributed tracing
        Activity.Current?.SetTag("correlation.id", correlationId);

        // Add to response header
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Add to Serilog LogContext (will be available after Serilog configuration)
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Extension methods for registering correlation ID middleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Gets the correlation ID from the current HttpContext.
    /// </summary>
    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue("CorrelationId", out var correlationId)
            ? correlationId?.ToString()
            : null;
    }
}
