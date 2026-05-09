using FinLedger.Gateway.Api.Middleware;

namespace FinLedger.Gateway.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGatewayMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
        return app;
    }
}
