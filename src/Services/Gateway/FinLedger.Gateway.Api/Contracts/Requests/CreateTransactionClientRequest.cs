namespace FinLedger.Gateway.Api.Contracts.Requests;

public sealed record CreateTransactionClientRequest(
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
    string? ExternalReference);
