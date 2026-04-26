namespace FinLedger.Contracts.BankIntegration;

public sealed record RegisterBankConnectionRequest(string BankCode, string DisplayName, string BaseUrl);

public sealed record BankConnectionDto(Guid Id, string BankCode, string DisplayName, string BaseUrl, string Status, DateTimeOffset CreatedAt);

public sealed record BankVerificationRequestDto(string BankCode, string AccountNumber);

public sealed record BankVerificationResultDto(string BankCode, string AccountNumber, bool IsAvailable, string Message);