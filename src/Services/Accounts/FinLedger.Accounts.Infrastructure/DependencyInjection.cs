using FinLedger.Accounts.Application.Abstractions;
using FinLedger.Accounts.Application.UseCases;
using FinLedger.Accounts.Infrastructure.Persistence;
using FinLedger.Accounts.Infrastructure.Repositories;
using FinLedger.Accounts.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinLedger.Accounts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ServiceDb")
            ?? "Server=localhost,1433;Database=FinLedger_Accounts;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

        services.AddDbContext<AccountsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IAccountRepository, EfAccountRepository>();
        services.AddScoped<IAccountNumberGenerator, AccountNumberGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IAccountUseCaseService, AccountUseCaseService>();

        return services;
    }
}
