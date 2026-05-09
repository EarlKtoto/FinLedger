using FinLedger.Gateway.Api.Clients;
using FinLedger.Gateway.Api.Options;
using Microsoft.Extensions.Options;

namespace FinLedger.Gateway.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceUrlOptions>(configuration.GetSection(ServiceUrlOptions.SectionName));
        services.Configure<GatewayApiKeyOptions>(options =>
        {
            options.Keys = configuration.GetSection(GatewayApiKeyOptions.SectionName).Get<string[]>() ?? [];
        });

        services.AddHttpContextAccessor();
        services.AddHttpClient<ITransactionsClient, TransactionsClient>((provider, client) =>
        {
            var serviceUrls = provider.GetRequiredService<IOptions<ServiceUrlOptions>>().Value;
            client.BaseAddress = new Uri(serviceUrls.Transactions.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
