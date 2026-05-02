namespace FinLedger.Ledger.Domain.Entities;

public sealed class AccountBalance
{
    private AccountBalance()
    {
    }

    private AccountBalance(Guid ledgerAccountId, decimal availableBalance, string currencyCode, DateTimeOffset updatedAtUtc)
    {
        Id = Guid.NewGuid();
        LedgerAccountId = ledgerAccountId;
        AvailableBalance = availableBalance;
        ReservedBalance = 0;
        CurrencyCode = NormalizeCurrency(currencyCode);
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid LedgerAccountId { get; private set; }

    public LedgerAccount? LedgerAccount { get; private set; }

    public decimal AvailableBalance { get; private set; }

    public decimal ReservedBalance { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public byte[] Version { get; private set; } = [];

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static AccountBalance Create(Guid ledgerAccountId, decimal initialAvailableBalance, string currencyCode, DateTimeOffset updatedAtUtc)
    {
        if (initialAvailableBalance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialAvailableBalance), "Initial available balance cannot be negative.");
        }

        return new AccountBalance(ledgerAccountId, initialAvailableBalance, currencyCode, updatedAtUtc);
    }

    public void DebitAvailable(decimal amount, DateTimeOffset updatedAtUtc)
    {
        EnsurePositive(amount);
        if (AvailableBalance - amount < 0)
        {
            throw new InvalidOperationException("AvailableBalance must never be negative.");
        }

        AvailableBalance -= amount;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void CreditAvailable(decimal amount, DateTimeOffset updatedAtUtc)
    {
        EnsurePositive(amount);
        AvailableBalance += amount;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Reserve(decimal amount, DateTimeOffset updatedAtUtc)
    {
        EnsurePositive(amount);
        if (AvailableBalance - amount < 0)
        {
            throw new InvalidOperationException("AvailableBalance must never be negative.");
        }

        AvailableBalance -= amount;
        ReservedBalance += amount;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Release(decimal amount, DateTimeOffset updatedAtUtc)
    {
        EnsurePositive(amount);
        if (ReservedBalance - amount < 0)
        {
            throw new InvalidOperationException("ReservedBalance must never be negative.");
        }

        ReservedBalance -= amount;
        AvailableBalance += amount;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Capture(decimal amount, DateTimeOffset updatedAtUtc)
    {
        EnsurePositive(amount);
        if (ReservedBalance - amount < 0)
        {
            throw new InvalidOperationException("ReservedBalance must never be negative.");
        }

        ReservedBalance -= amount;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void EnsurePositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        return currencyCode.Trim().ToUpperInvariant();
    }
}
