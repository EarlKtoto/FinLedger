using FinLedger.Accounts.Domain.Entities;
using FinLedger.Accounts.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Accounts.Infrastructure.Persistence;

public sealed class AccountsDbContext : DbContext
{
    public AccountsDbContext(DbContextOptions<AccountsDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<AccountLimit> AccountLimits => Set<AccountLimit>();

    public DbSet<AccountStatusHistory> AccountStatusHistory => Set<AccountStatusHistory>();

    public DbSet<AccountNumberSequence> AccountNumberSequences => Set<AccountNumberSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.AccountNumber).IsUnique();
            entity.HasIndex(x => x.ParticipantId);
            entity.Property(x => x.AccountNumber).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).HasDefaultValue(AccountStatus.PendingActivation);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AllowIncomingPayments).IsRequired();
            entity.Property(x => x.AllowOutgoingPayments).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Navigation(x => x.Limits).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(x => x.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany(x => x.Limits)
                .WithOne(x => x.Account)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.StatusHistory)
                .WithOne(x => x.Account)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AccountLimit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.AccountId, x.LimitType }).IsUnique();
            entity.Property(x => x.LimitType).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<AccountStatusHistory>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.AccountId);
            entity.Property(x => x.PreviousStatus).HasConversion<string>().HasMaxLength(64);
            entity.Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ChangedAtUtc).IsRequired();
        });

        modelBuilder.Entity<AccountNumberSequence>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Prefix).IsUnique();
            entity.Property(x => x.Prefix).HasMaxLength(8).IsRequired();
            entity.Property(x => x.NextValue).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });
    }
}
