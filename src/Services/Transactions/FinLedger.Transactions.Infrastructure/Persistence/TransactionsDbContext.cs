using FinLedger.Transactions.Domain.Entities;
using FinLedger.Transactions.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Transactions.Infrastructure.Persistence;

public sealed class TransactionsDbContext : DbContext
{
    public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<TransactionHistory> TransactionHistory => Set<TransactionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionHistoryConfiguration());
    }
}
