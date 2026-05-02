using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace FinLedger.Transactions.Infrastructure.Clients;

public sealed class DownstreamAuthorizationHandler : DelegatingHandler
{
    private readonly IConfiguration _configuration;

    public DownstreamAuthorizationHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var bearerToken = _configuration["DownstreamServices:BearerToken"];
        if (!string.IsNullOrWhiteSpace(bearerToken) && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
