using FinLedger.Ledger.Domain.Enums;

namespace FinLedger.Ledger.Domain.Entities;

public sealed class LedgerAccount
{
    private LedgerAccount()
    {
    }

    private LedgerAccount(Guid accountId, Guid participantId, string accountNumber, string currencyCode, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        ParticipantId = participantId;
        AccountNumber = accountNumber;
        CurrencyCode = NormalizeCurrency(currencyCode);
        Status = LedgerAccountStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Guid ParticipantId { get; private set; }

    public string AccountNumber { get; private set; } = string.Empty;

    public string CurrencyCode { get; private set; } = string.Empty;

    public LedgerAccountStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static LedgerAccount Register(Guid accountId, Guid participantId, string accountNumber, string currencyCode, DateTimeOffset createdAtUtc)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("AccountId is required.", nameof(accountId));
        }

        if (participantId == Guid.Empty)
        {
            throw new ArgumentException("ParticipantId is required.", nameof(participantId));
        }

        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new ArgumentException("AccountNumber is required.", nameof(accountNumber));
        }

        return new LedgerAccount(accountId, participantId, accountNumber.Trim(), currencyCode, createdAtUtc);
    }

    public void EnsureActive()
    {
        if (Status != LedgerAccountStatus.Active)
        {
            throw new InvalidOperationException("Ledger account must be Active.");
        }
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        return currencyCode.Trim().ToUpperInvariant();
    }
}
