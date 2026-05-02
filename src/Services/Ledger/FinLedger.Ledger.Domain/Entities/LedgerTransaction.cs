using FinLedger.Ledger.Domain.Enums;

namespace FinLedger.Ledger.Domain.Entities;

public sealed class LedgerTransaction
{
    private readonly List<LedgerEntry> _entries = [];

    private LedgerTransaction()
    {
    }

    private LedgerTransaction(
        string externalTransactionId,
        string idempotencyKey,
        LedgerTransactionType type,
        decimal amount,
        string currencyCode,
        string? description,
        Guid? reversedTransactionId,
        DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        ExternalTransactionId = externalTransactionId;
        IdempotencyKey = idempotencyKey;
        Type = type;
        Status = LedgerTransactionStatus.Pending;
        Amount = amount;
        CurrencyCode = NormalizeCurrency(currencyCode);
        Description = description;
        CreatedAtUtc = createdAtUtc;
        ReversedTransactionId = reversedTransactionId;
    }

    public Guid Id { get; private set; }

    public string ExternalTransactionId { get; private set; } = string.Empty;

    public string IdempotencyKey { get; private set; } = string.Empty;

    public LedgerTransactionType Type { get; private set; }

    public LedgerTransactionStatus Status { get; private set; }

    public decimal Amount { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? FailedAtUtc { get; private set; }

    public Guid? ReversedTransactionId { get; private set; }

    public IReadOnlyCollection<LedgerEntry> Entries => _entries;

    public static LedgerTransaction Create(
        string externalTransactionId,
        string idempotencyKey,
        LedgerTransactionType type,
        decimal amount,
        string currencyCode,
        string? description,
        DateTimeOffset createdAtUtc,
        Guid? reversedTransactionId = null)
    {
        if (string.IsNullOrWhiteSpace(externalTransactionId))
        {
            throw new ArgumentException("ExternalTransactionId is required.", nameof(externalTransactionId));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("IdempotencyKey is required.", nameof(idempotencyKey));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        return new LedgerTransaction(externalTransactionId.Trim(), idempotencyKey.Trim(), type, amount, currencyCode, description, reversedTransactionId, createdAtUtc);
    }

    public LedgerEntry AddEntry(Guid ledgerAccountId, LedgerEntryDirection direction, decimal amount, DateTimeOffset createdAtUtc)
    {
        var entry = LedgerEntry.Create(Id, ledgerAccountId, direction, amount, CurrencyCode, createdAtUtc);
        _entries.Add(entry);
        return entry;
    }

    public void Complete(DateTimeOffset completedAtUtc)
    {
        EnsureBalanced();
        Status = LedgerTransactionStatus.Completed;
        CompletedAtUtc = completedAtUtc;
    }

    public void Fail(DateTimeOffset failedAtUtc)
    {
        Status = LedgerTransactionStatus.Failed;
        FailedAtUtc = failedAtUtc;
    }

    public void MarkReversed(DateTimeOffset reversedAtUtc)
    {
        if (Status == LedgerTransactionStatus.Reversed)
        {
            throw new InvalidOperationException("Ledger transaction cannot be reversed twice.");
        }

        Status = LedgerTransactionStatus.Reversed;
        CompletedAtUtc ??= reversedAtUtc;
    }

    public void EnsureBalanced()
    {
        var debitTotal = _entries.Where(x => x.Direction == LedgerEntryDirection.Debit).Sum(x => x.Amount);
        var creditTotal = _entries.Where(x => x.Direction == LedgerEntryDirection.Credit).Sum(x => x.Amount);
        if (debitTotal != creditTotal)
        {
            throw new InvalidOperationException("Debit total must equal credit total.");
        }
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        return currencyCode.Trim().ToUpperInvariant();
    }
}
