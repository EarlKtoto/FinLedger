namespace FinLedger.Identity.Domain.Entities;

public sealed class ApiKey
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApiClientId { get; set; }

    public ApiClient? ApiClient { get; set; }

    public string Name { get; set; } = string.Empty;

    public string KeyPrefix { get; set; } = string.Empty;

    public string KeyHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsExpired(DateTimeOffset utcNow) => ExpiresAtUtc.HasValue && ExpiresAtUtc <= utcNow;

    public bool IsActive(DateTimeOffset utcNow) => !IsRevoked && !IsExpired(utcNow);
}
