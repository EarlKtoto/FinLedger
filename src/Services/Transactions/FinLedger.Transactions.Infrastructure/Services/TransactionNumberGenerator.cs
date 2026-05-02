using FinLedger.Transactions.Application.Abstractions;

namespace FinLedger.Transactions.Infrastructure.Services;

public sealed class TransactionNumberGenerator : ITransactionNumberGenerator
{
    public string Generate()
    {
        return $"TX-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..31].ToUpperInvariant();
    }
}
