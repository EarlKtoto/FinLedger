namespace FinLedger.Gateway.Api.Contracts.Responses;

public sealed record PaymentResponse(
    Guid TransactionId,
    string TransactionNumber,
    string Status,
    Guid PayerParticipantId,
    Guid PayerAccountId,
    Guid ReceiverParticipantId,
    Guid ReceiverAccountId,
    decimal Amount,
    string Currency,
    string? Description,
    string? ExternalReference,
    string? FailureReason,
    Guid? LedgerReservationId,
    Guid? LedgerTransactionId,
    string CorrelationId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc);
