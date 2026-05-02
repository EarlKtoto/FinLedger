using FinLedger.BankIntegration.Domain.Entities;
using FinLedger.BankIntegration.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinLedger.BankIntegration.Infrastructure.Persistence.Configurations;

public sealed class BankConnectionConfiguration : IEntityTypeConfiguration<BankConnection>
{
    public void Configure(EntityTypeBuilder<BankConnection> builder)
    {
        builder.ToTable("BankConnections");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ParticipantId).IsUnique();
        builder.HasIndex(x => x.BankCode);
        builder.Property(x => x.BankCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.BaseUrl).HasMaxLength(512).IsRequired();
        builder.Property(x => x.ApiKey).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).HasDefaultValue(BankConnectionStatus.Active);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
