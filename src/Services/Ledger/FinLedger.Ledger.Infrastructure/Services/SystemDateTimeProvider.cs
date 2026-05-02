using FinLedger.Ledger.Application.Abstractions;

namespace FinLedger.Ledger.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
