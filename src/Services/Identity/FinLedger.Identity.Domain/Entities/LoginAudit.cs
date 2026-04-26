namespace FinLedger.Identity.Domain.Entities;

public sealed class LoginAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public string Email { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public string? FailureReason { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
