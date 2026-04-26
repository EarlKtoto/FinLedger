namespace FinLedger.Contracts.Accounts;

public sealed record CreateAccountRequest(string Reference, string Description, decimal Amount);

public sealed record AccountDto(Guid Id, string Reference, string Description, decimal Amount, string Status, DateTimeOffset CreatedAt);
