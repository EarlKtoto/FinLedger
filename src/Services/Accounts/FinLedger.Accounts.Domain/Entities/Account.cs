using FinLedger.Accounts.Domain.Enums;

namespace FinLedger.Accounts.Domain.Entities;

public sealed class Account
{
    private readonly List<AccountLimit> _limits = [];
    private readonly List<AccountStatusHistory> _statusHistory = [];

    private Account()
    {
    }

    private Account(
        Guid participantId,
        string accountNumber,
        AccountType type,
        string currencyCode,
        string displayName,
        bool allowIncomingPayments,
        bool allowOutgoingPayments,
        DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        ParticipantId = participantId;
        AccountNumber = accountNumber;
        Type = type;
        Status = AccountStatus.PendingActivation;
        CurrencyCode = NormalizeCurrency(currencyCode);
        DisplayName = displayName;
        AllowIncomingPayments = allowIncomingPayments;
        AllowOutgoingPayments = allowOutgoingPayments;
        CreatedAtUtc = createdAtUtc;
        AddStatusHistory(null, Status, "Account created", createdAtUtc);
    }

    public Guid Id { get; private set; }

    public Guid ParticipantId { get; private set; }

    public string AccountNumber { get; private set; } = string.Empty;

    public AccountType Type { get; private set; }

    public AccountStatus Status { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public bool AllowIncomingPayments { get; private set; }

    public bool AllowOutgoingPayments { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public IReadOnlyCollection<AccountLimit> Limits => _limits;

    public IReadOnlyCollection<AccountStatusHistory> StatusHistory => _statusHistory;

    public static Account Create(
        Guid participantId,
        string accountNumber,
        AccountType type,
        string currencyCode,
        string displayName,
        bool allowIncomingPayments,
        bool allowOutgoingPayments,
        DateTimeOffset createdAtUtc)
    {
        return new Account(participantId, accountNumber, type, currencyCode, displayName, allowIncomingPayments, allowOutgoingPayments, createdAtUtc);
    }

    public void Update(string displayName, bool allowIncomingPayments, bool allowOutgoingPayments, DateTimeOffset updatedAtUtc)
    {
        EnsureNotClosed();
        DisplayName = displayName;
        AllowIncomingPayments = allowIncomingPayments;
        AllowOutgoingPayments = allowOutgoingPayments;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Activate(DateTimeOffset changedAtUtc)
    {
        ChangeStatus(AccountStatus.Active, "Account activated", changedAtUtc);
    }

    public void Suspend(DateTimeOffset changedAtUtc)
    {
        ChangeStatus(AccountStatus.Suspended, "Account suspended", changedAtUtc);
    }

    public void Freeze(DateTimeOffset changedAtUtc)
    {
        ChangeStatus(AccountStatus.Frozen, "Account frozen", changedAtUtc);
    }

    public void Close(DateTimeOffset changedAtUtc, string? reason)
    {
        ChangeStatus(AccountStatus.Closed, string.IsNullOrWhiteSpace(reason) ? "Account closed" : reason, changedAtUtc);
        ClosedAtUtc = changedAtUtc;
    }

    public AccountLimit SetLimit(LimitType limitType, decimal amount, DateTimeOffset changedAtUtc)
    {
        EnsureNotClosed();

        var existing = _limits.FirstOrDefault(x => x.LimitType == limitType);
        if (existing is not null)
        {
            existing.Update(amount, changedAtUtc);
            UpdatedAtUtc = changedAtUtc;
            return existing;
        }

        var limit = AccountLimit.Create(Id, limitType, amount, CurrencyCode, changedAtUtc);
        _limits.Add(limit);
        UpdatedAtUtc = changedAtUtc;
        return limit;
    }

    public void RemoveLimit(Guid limitId, DateTimeOffset changedAtUtc)
    {
        EnsureNotClosed();

        var limit = _limits.FirstOrDefault(x => x.Id == limitId);
        if (limit is null)
        {
            return;
        }

        _limits.Remove(limit);
        UpdatedAtUtc = changedAtUtc;
    }

    private void ChangeStatus(AccountStatus newStatus, string reason, DateTimeOffset changedAtUtc)
    {
        if (Status == AccountStatus.Closed)
        {
            throw new InvalidOperationException("Closed accounts cannot change status.");
        }

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = changedAtUtc;
        AddStatusHistory(oldStatus, newStatus, reason, changedAtUtc);
    }

    private void AddStatusHistory(AccountStatus? oldStatus, AccountStatus newStatus, string reason, DateTimeOffset changedAtUtc)
    {
        _statusHistory.Add(AccountStatusHistory.Create(Id, oldStatus, newStatus, reason, changedAtUtc));
    }

    private void EnsureNotClosed()
    {
        if (Status == AccountStatus.Closed)
        {
            throw new InvalidOperationException("Closed accounts cannot be modified.");
        }
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        return currencyCode.Trim().ToUpperInvariant();
    }
}
