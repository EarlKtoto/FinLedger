using System.Net;
using FinLedger.Ledger.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Ledger.Api.Middleware;

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
            var statusCode = GetStatusCode(exception);
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception");
            }

            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = statusCode.ToString(),
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            LedgerValidationException or ArgumentOutOfRangeException or ArgumentException => HttpStatusCode.BadRequest,
            LedgerNotFoundException => HttpStatusCode.NotFound,
            LedgerConflictException or InvalidOperationException or DbUpdateConcurrencyException or DbUpdateException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };
    }
}
