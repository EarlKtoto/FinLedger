using FinLedger.BankIntegration.Application.Abstractions;
using FinLedger.BankIntegration.Application.UseCases;
using FinLedger.BankIntegration.Infrastructure.Clients;
using FinLedger.BankIntegration.Infrastructure.Persistence;
using FinLedger.BankIntegration.Infrastructure.Repositories;
using FinLedger.BankIntegration.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinLedger.BankIntegration.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBankIntegrationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ServiceDb")
            ?? "Server=localhost,1433;Database=FinLedger_BankIntegration;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

        services.AddDbContext<BankIntegrationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IBankConnectionRepository, BankConnectionRepository>();
        services.AddScoped<IBankIntegrationRequestLogRepository, BankIntegrationRequestLogRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IBankIntegrationUseCaseService, BankIntegrationUseCaseService>();
        services.AddHttpClient<IExternalBankClient, ExternalBankClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue("ExternalBank:TimeoutSeconds", 10));
        });

        return services;
    }
}
