using FinLedger.Ledger.Domain.Enums;

namespace FinLedger.Ledger.Domain.Entities;

public sealed class LedgerEntry
{
    private LedgerEntry()
    {
    }

    private LedgerEntry(Guid ledgerTransactionId, Guid ledgerAccountId, LedgerEntryDirection direction, decimal amount, string currencyCode, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        LedgerTransactionId = ledgerTransactionId;
        LedgerAccountId = ledgerAccountId;
        Direction = direction;
        Amount = amount;
        CurrencyCode = NormalizeCurrency(currencyCode);
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid LedgerTransactionId { get; private set; }

    public LedgerTransaction? LedgerTransaction { get; private set; }

    public Guid LedgerAccountId { get; private set; }

    public LedgerAccount? LedgerAccount { get; private set; }

    public LedgerEntryDirection Direction { get; private set; }

    public decimal Amount { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static LedgerEntry Create(Guid ledgerTransactionId, Guid ledgerAccountId, LedgerEntryDirection direction, decimal amount, string currencyCode, DateTimeOffset createdAtUtc)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        return new LedgerEntry(ledgerTransactionId, ledgerAccountId, direction, amount, currencyCode, createdAtUtc);
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        return currencyCode.Trim().ToUpperInvariant();
    }
}
