using FinLedger.Accounts.Application.Abstractions;

namespace FinLedger.Accounts.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
