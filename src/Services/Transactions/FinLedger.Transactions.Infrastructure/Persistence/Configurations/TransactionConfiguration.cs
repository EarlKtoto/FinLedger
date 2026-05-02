using FinLedger.Transactions.Domain.Entities;
using FinLedger.Transactions.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinLedger.Transactions.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TransactionNumber).IsUnique();
        builder.HasIndex(x => x.ExternalReference);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PayerParticipantId);
        builder.HasIndex(x => x.ReceiverParticipantId);

        builder.Property(x => x.TransactionNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PayerBankCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.PayerAccountNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ReceiverBankCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ReceiverAccountNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.ExternalReference).HasMaxLength(128);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).HasDefaultValue(TransactionStatus.Created);
        builder.Property(x => x.FailureReason).HasMaxLength(1024);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Navigation(x => x.History).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.History)
            .WithOne(x => x.Transaction)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
