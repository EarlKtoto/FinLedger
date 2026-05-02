using FinLedger.BankIntegration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinLedger.BankIntegration.Infrastructure.Persistence.Configurations;

public sealed class BankIntegrationRequestLogConfiguration : IEntityTypeConfiguration<BankIntegrationRequestLog>
{
    public void Configure(EntityTypeBuilder<BankIntegrationRequestLog> builder)
    {
        builder.ToTable("BankIntegrationRequestLogs");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ParticipantId);
        builder.HasIndex(x => x.BankConnectionId);
        builder.HasIndex(x => x.CreatedAtUtc);
        builder.Property(x => x.ValidationType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.RequestUrl).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.RequestPayload).HasMaxLength(4096).IsRequired();
        builder.Property(x => x.ResponsePayload).HasMaxLength(4096);
        builder.Property(x => x.ErrorCode).HasMaxLength(64);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1024);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasOne(x => x.BankConnection)
            .WithMany()
            .HasForeignKey(x => x.BankConnectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
