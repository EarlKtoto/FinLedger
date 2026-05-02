using FinLedger.Accounts.Domain.Enums;

namespace FinLedger.Accounts.Domain.Entities;

public sealed class AccountLimit
{
    private AccountLimit()
    {
    }

    private AccountLimit(Guid accountId, LimitType limitType, decimal amount, string currencyCode, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        LimitType = limitType;
        Amount = amount;
        CurrencyCode = currencyCode;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Account? Account { get; private set; }

    public LimitType LimitType { get; private set; }

    public decimal Amount { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static AccountLimit Create(Guid accountId, LimitType limitType, decimal amount, string currencyCode, DateTimeOffset createdAtUtc)
    {
        ValidateAmount(amount);
        return new AccountLimit(accountId, limitType, amount, currencyCode, createdAtUtc);
    }

    public void Update(decimal amount, DateTimeOffset updatedAtUtc)
    {
        ValidateAmount(amount);
        Amount = amount;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Limit amount must be positive.");
        }
    }
}
