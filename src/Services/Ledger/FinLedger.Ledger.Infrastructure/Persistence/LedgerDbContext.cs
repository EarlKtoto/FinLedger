using FinLedger.Ledger.Domain.Entities;
using FinLedger.Ledger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Ledger.Infrastructure.Persistence;

public sealed class LedgerDbContext : DbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options) { }

    public DbSet<LedgerAccount> LedgerAccounts => Set<LedgerAccount>();

    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();

    public DbSet<LedgerTransaction> LedgerTransactions => Set<LedgerTransaction>();

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    public DbSet<FundsReservation> FundsReservations => Set<FundsReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerAccount>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.AccountId).IsUnique();
            entity.Property(x => x.AccountNumber).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).HasDefaultValue(LedgerAccountStatus.Active);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<AccountBalance>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.LedgerAccountId).IsUnique();
            entity.Property(x => x.AvailableBalance).HasPrecision(18, 2);
            entity.Property(x => x.ReservedBalance).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Version).IsRowVersion();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasOne(x => x.LedgerAccount)
                .WithOne()
                .HasForeignKey<AccountBalance>(x => x.LedgerAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LedgerTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.ExternalTransactionId);
            entity.Property(x => x.ExternalTransactionId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Navigation(x => x.Entries).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany(x => x.Entries)
                .WithOne(x => x.LedgerTransaction)
                .HasForeignKey(x => x.LedgerTransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.LedgerAccountId);
            entity.HasIndex(x => x.LedgerTransactionId);
            entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasOne(x => x.LedgerAccount)
                .WithMany()
                .HasForeignKey(x => x.LedgerAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FundsReservation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ExternalTransactionId);
            entity.Property(x => x.ExternalTransactionId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasOne(x => x.LedgerAccount)
                .WithMany()
                .HasForeignKey(x => x.LedgerAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
