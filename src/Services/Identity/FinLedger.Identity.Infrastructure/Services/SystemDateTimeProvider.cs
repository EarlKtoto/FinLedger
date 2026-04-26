using FinLedger.Identity.Application.Abstractions;

namespace FinLedger.Identity.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
