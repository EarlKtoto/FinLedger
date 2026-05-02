using FinLedger.Accounts.Domain.Enums;

namespace FinLedger.Accounts.Domain.Entities;

public sealed class AccountStatusHistory
{
    private AccountStatusHistory()
    {
    }

    private AccountStatusHistory(Guid accountId, AccountStatus? previousStatus, AccountStatus newStatus, string reason, DateTimeOffset changedAtUtc)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
        ChangedAtUtc = changedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Account? Account { get; private set; }

    public AccountStatus? PreviousStatus { get; private set; }

    public AccountStatus NewStatus { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public DateTimeOffset ChangedAtUtc { get; private set; }

    public static AccountStatusHistory Create(Guid accountId, AccountStatus? previousStatus, AccountStatus newStatus, string reason, DateTimeOffset changedAtUtc)
    {
        return new AccountStatusHistory(accountId, previousStatus, newStatus, reason, changedAtUtc);
    }
}
