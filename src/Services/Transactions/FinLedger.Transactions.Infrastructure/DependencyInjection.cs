using FinLedger.Transactions.Application.Abstractions;
using FinLedger.Transactions.Application.UseCases;
using FinLedger.Transactions.Infrastructure.Clients;
using FinLedger.Transactions.Infrastructure.Persistence;
using FinLedger.Transactions.Infrastructure.Repositories;
using FinLedger.Transactions.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinLedger.Transactions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTransactionsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ServiceDb")
            ?? "Server=localhost,1433;Database=FinLedger_Transactions;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

        services.AddDbContext<TransactionsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<ITransactionNumberGenerator, TransactionNumberGenerator>();
        services.AddScoped<ITransactionUseCaseService, TransactionUseCaseService>();
        services.AddTransient<DownstreamAuthorizationHandler>();

        services.AddHttpClient<IBankIntegrationClient, HttpBankIntegrationClient>(client =>
            client.BaseAddress = new Uri(configuration["DownstreamServices:BankIntegration"] ?? "http://localhost:5028"))
            .AddHttpMessageHandler<DownstreamAuthorizationHandler>();
        services.AddHttpClient<ILedgerClient, HttpLedgerClient>(client =>
            client.BaseAddress = new Uri(configuration["DownstreamServices:Ledger"] ?? "http://localhost:5077"))
            .AddHttpMessageHandler<DownstreamAuthorizationHandler>();
        services.AddHttpClient<IAuditClient, HttpAuditClient>(client =>
            client.BaseAddress = new Uri(configuration["DownstreamServices:Audit"] ?? "http://localhost:5195"))
            .AddHttpMessageHandler<DownstreamAuthorizationHandler>();

        return services;
    }
}
