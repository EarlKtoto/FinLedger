using FinLedger.BankIntegration.Domain.Entities;
using FinLedger.BankIntegration.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.BankIntegration.Infrastructure.Persistence;

public sealed class BankIntegrationDbContext : DbContext
{
    public BankIntegrationDbContext(DbContextOptions<BankIntegrationDbContext> options) : base(options) { }

    public DbSet<BankConnection> BankConnections => Set<BankConnection>();

    public DbSet<BankIntegrationRequestLog> BankIntegrationRequestLogs => Set<BankIntegrationRequestLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BankConnectionConfiguration());
        modelBuilder.ApplyConfiguration(new BankIntegrationRequestLogConfiguration());
    }
}
