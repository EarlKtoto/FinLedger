using System.ComponentModel.DataAnnotations;

namespace FinLedger.Contracts.Ledger;

public sealed record LedgerAccountDto(
    Guid Id,
    Guid AccountId,
    Guid ParticipantId,
    string AccountNumber,
    string CurrencyCode,
    string Status,
    DateTimeOffset CreatedAtUtc);

public sealed record AccountBalanceDto(
    Guid Id,
    Guid LedgerAccountId,
    Guid AccountId,
    decimal AvailableBalance,
    decimal ReservedBalance,
    string CurrencyCode,
    DateTimeOffset UpdatedAtUtc);

public sealed record LedgerEntryDto(
    Guid Id,
    Guid LedgerTransactionId,
    Guid LedgerAccountId,
    Guid AccountId,
    string Direction,
    decimal Amount,
    string CurrencyCode,
    DateTimeOffset CreatedAtUtc);

public sealed record LedgerTransactionDto(
    Guid Id,
    string ExternalTransactionId,
    string IdempotencyKey,
    string Type,
    string Status,
    decimal Amount,
    string CurrencyCode,
    string? Description,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? FailedAtUtc,
    Guid? ReversedTransactionId,
    IReadOnlyCollection<LedgerEntryDto> Entries);

public sealed record FundsReservationDto(
    Guid Id,
    string ExternalTransactionId,
    Guid LedgerAccountId,
    Guid AccountId,
    decimal Amount,
    string CurrencyCode,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ConfirmedAtUtc,
    DateTimeOffset? ReleasedAtUtc,
    DateTimeOffset? ExpiresAtUtc);

public sealed record RegisterLedgerAccountRequest(
    Guid AccountId,
    Guid ParticipantId,
    [Required] string AccountNumber,
    [Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    decimal InitialAvailableBalance = 0);

public sealed record ReserveFundsRequest(
    Guid AccountId,
    [Required] string ExternalTransactionId,
    [Required] string IdempotencyKey,
    decimal Amount,
    [Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    DateTimeOffset? ExpiresAtUtc,
    string? Description);

public sealed record ReleaseReservationRequest(
    [Required] string IdempotencyKey,
    string? Description);

public sealed record CaptureReservationRequest(
    Guid CreditAccountId,
    [Required] string IdempotencyKey,
    string? ExternalTransactionId,
    string? Description);

public sealed record CreateTransferRequest(
    Guid DebitAccountId,
    Guid CreditAccountId,
    [Required] string ExternalTransactionId,
    [Required] string IdempotencyKey,
    decimal Amount,
    [Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    string? Description);

public sealed record ReverseLedgerTransactionRequest(
    [Required] string IdempotencyKey,
    string? ExternalTransactionId,
    string? Description);

public sealed record LedgerTransferRequestDto(
    Guid PayerLedgerAccountId,
    Guid RecipientLedgerAccountId,
    decimal Amount,
    string Currency,
    string ExternalReference);

public sealed record LedgerTransferResultDto(Guid TransferId, bool Succeeded, string Message);
