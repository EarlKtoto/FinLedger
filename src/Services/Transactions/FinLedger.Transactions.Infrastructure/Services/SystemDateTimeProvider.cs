using FinLedger.Transactions.Application.Abstractions;

namespace FinLedger.Transactions.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
