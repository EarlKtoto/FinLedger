using FinLedger.Identity.Domain.Entities;
using FinLedger.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<ApiClient> ApiClients => Set<ApiClient>();

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    public DbSet<LoginAudit> LoginAudits => Set<LoginAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ParticipantId);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).HasDefaultValue(UserStatus.Active);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc);
            entity.Property(x => x.LastLoginAtUtc);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreatedByIp).HasMaxLength(64);
            entity.Property(x => x.RevokedByIp).HasMaxLength(64);
            entity.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiClient>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ClientId).IsUnique();
            entity.Property(x => x.ClientId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).HasDefaultValue(ApiClientStatus.Active);
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.KeyHash).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.KeyPrefix).HasMaxLength(32).IsRequired();
            entity.Property(x => x.KeyHash).HasMaxLength(128).IsRequired();
            entity.HasOne(x => x.ApiClient)
                .WithMany(x => x.ApiKeys)
                .HasForeignKey(x => x.ApiClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoginAudit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(512);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.HasOne(x => x.User)
                .WithMany(x => x.LoginAudits)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
