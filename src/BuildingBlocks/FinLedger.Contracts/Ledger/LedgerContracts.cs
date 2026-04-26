namespace FinLedger.Contracts.Ledger;

public sealed record CreateLedgerAccountRequest(Guid OwnerId, string Currency, decimal OpeningBalance);

public sealed record LedgerAccountDto(Guid Id, Guid OwnerId, string Currency, decimal Balance, DateTimeOffset CreatedAt);

public sealed record LedgerTransferRequestDto(Guid PayerLedgerAccountId, Guid RecipientLedgerAccountId, decimal Amount, string Currency, string ExternalReference);

public sealed record LedgerTransferResultDto(Guid TransferId, bool Succeeded, string Message);