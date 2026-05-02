using FinLedger.Ledger.Application.Abstractions;
using FinLedger.Ledger.Application.UseCases;
using FinLedger.Ledger.Infrastructure.Persistence;
using FinLedger.Ledger.Infrastructure.Repositories;
using FinLedger.Ledger.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinLedger.Ledger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLedgerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ServiceDb")
            ?? "Server=localhost,1433;Database=FinLedger_Ledger;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

        services.AddDbContext<LedgerDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ILedgerRepository, EfLedgerRepository>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ILedgerUseCaseService, LedgerUseCaseService>();

        return services;
    }
}
