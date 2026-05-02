namespace FinLedger.Accounts.Domain.Entities;

public sealed class AccountNumberSequence
{
    private AccountNumberSequence()
    {
    }

    private AccountNumberSequence(string prefix, long nextValue, DateTimeOffset updatedAtUtc)
    {
        Id = Guid.NewGuid();
        Prefix = prefix;
        NextValue = nextValue;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Prefix { get; private set; } = string.Empty;

    public long NextValue { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static AccountNumberSequence Create(string prefix, DateTimeOffset createdAtUtc)
    {
        return new AccountNumberSequence(prefix, 1, createdAtUtc);
    }

    public string GenerateNext(DateTimeOffset updatedAtUtc)
    {
        var accountNumber = $"{Prefix}-{NextValue:0000000000}";
        NextValue++;
        UpdatedAtUtc = updatedAtUtc;
        return accountNumber;
    }
}
