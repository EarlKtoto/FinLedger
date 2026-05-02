using System.ComponentModel.DataAnnotations;

namespace FinLedger.Contracts.Transactions;

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2
}

public sealed record PaymentRequestDto(
    string PayerBankCode,
    string PayerAccountNumber,
    string RecipientBankCode,
    string RecipientAccountNumber,
    decimal Amount,
    string Currency,
    string ExternalReference);

public sealed record PaymentResultDto(Guid TransactionId, PaymentStatus Status, string Message);

public sealed record CreateTransactionRequest(
    Guid PayerParticipantId,
    Guid ReceiverParticipantId,
    Guid PayerAccountId,
    Guid ReceiverAccountId,
    [Required] string PayerBankCode,
    [Required] string PayerAccountNumber,
    [Required] string ReceiverBankCode,
    [Required] string ReceiverAccountNumber,
    decimal Amount,
    [Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    string? Description,
    string? ExternalReference);

public sealed record CancelTransactionRequest(string? Reason);

public sealed record TransactionDto(
    Guid Id,
    string TransactionNumber,
    Guid PayerParticipantId,
    Guid ReceiverParticipantId,
    Guid PayerAccountId,
    Guid ReceiverAccountId,
    string PayerBankCode,
    string PayerAccountNumber,
    string ReceiverBankCode,
    string ReceiverAccountNumber,
    decimal Amount,
    string CurrencyCode,
    string? Description,
    string? ExternalReference,
    string Status,
    Guid? LedgerReservationId,
    Guid? LedgerTransactionId,
    string? FailureReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    IReadOnlyCollection<TransactionHistoryDto> History);

public sealed record TransactionHistoryDto(
    Guid Id,
    Guid TransactionId,
    string Status,
    string? Reason,
    DateTimeOffset CreatedAtUtc);
