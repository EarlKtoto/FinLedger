using FinLedger.Identity.Domain.Enums;

namespace FinLedger.Identity.Domain.Entities;

public sealed class ApiClient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ClientId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Guid? ParticipantId { get; set; }

    public string? Description { get; set; }

    public ApiClientStatus Status { get; set; } = ApiClientStatus.Active;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public ICollection<ApiKey> ApiKeys { get; set; } = [];
}
