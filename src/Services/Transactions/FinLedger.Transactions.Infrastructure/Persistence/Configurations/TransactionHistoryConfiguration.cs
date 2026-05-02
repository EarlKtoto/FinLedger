using FinLedger.Transactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinLedger.Transactions.Infrastructure.Persistence.Configurations;

public sealed class TransactionHistoryConfiguration : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.ToTable("TransactionHistory");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TransactionId);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1024);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
