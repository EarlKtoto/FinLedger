using FinLedger.BankIntegration.Application.Abstractions;

namespace FinLedger.BankIntegration.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
