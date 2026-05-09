using FinLedger.Gateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Gateway.Infrastructure.Persistence;

public sealed class GatewayDbContext : DbContext
{
    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options) { }
    public DbSet<GatewayRoute> GatewayRoutes => Set<GatewayRoute>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GatewayRoute>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Reference).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
        });
    }
}