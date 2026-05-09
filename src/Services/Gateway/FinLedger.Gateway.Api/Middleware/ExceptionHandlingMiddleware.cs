using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Gateway.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            var (statusCode, title) = Map(exception);
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled Gateway exception");
            }

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
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

    private static (int StatusCode, string Title) Map(Exception exception)
    {
        return exception switch
        {
            GatewayException gatewayException => (gatewayException.StatusCode, gatewayException.Title),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            TaskCanceledException => (StatusCodes.Status504GatewayTimeout, "Gateway Timeout"),
            HttpRequestException => (StatusCodes.Status502BadGateway, "Bad Gateway"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
    }
}
