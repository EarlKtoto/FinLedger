namespace FinLedger.Gateway.Api.Contracts.Responses;

public sealed record TransactionDetailsClientResponse(
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
    IReadOnlyCollection<TransactionHistoryClientResponse> History);

public sealed record TransactionHistoryClientResponse(
    Guid Id,
    Guid TransactionId,
    string Status,
    string? Reason,
    DateTimeOffset CreatedAtUtc);
