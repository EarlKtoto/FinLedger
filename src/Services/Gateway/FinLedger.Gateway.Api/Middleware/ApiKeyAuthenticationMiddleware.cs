using FinLedger.Gateway.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinLedger.Gateway.Api.Middleware;

public sealed class ApiKeyAuthenticationMiddleware
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly IOptions<GatewayApiKeyOptions> _options;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        IOptions<GatewayApiKeyOptions> options)
    {
        _next = next;
        _environment = environment;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyValues) ||
            string.IsNullOrWhiteSpace(apiKeyValues.FirstOrDefault()))
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized", "API key is required.");
            return;
        }

        var apiKey = apiKeyValues.First();
        var allowedKeys = _options.Value.Keys;
        if (allowedKeys.Length == 0 || !allowedKeys.Contains(apiKey, StringComparer.Ordinal))
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Forbidden", "API key is invalid.");
            return;
        }

        await _next(context);
    }

    private bool ShouldSkip(PathString path)
    {
        return _environment.IsDevelopment() &&
               (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/openapi"));
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (context.Items.TryGetValue(CorrelationIdMiddleware.ItemName, out var correlationId))
        {
            problem.Extensions["correlationId"] = correlationId;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
