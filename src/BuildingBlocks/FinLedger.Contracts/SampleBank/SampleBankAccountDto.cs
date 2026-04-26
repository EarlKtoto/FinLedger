namespace FinLedger.Contracts.SampleBank;

public sealed record CreateSampleBankAccountRequest(string Reference, string Description, decimal Amount);

public sealed record SampleBankAccountDto(Guid Id, string Reference, string Description, decimal Amount, string Status, DateTimeOffset CreatedAt);
