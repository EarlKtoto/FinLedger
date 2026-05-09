namespace FinLedger.Gateway.Api.Contracts.Responses;

public sealed record CreateTransactionClientResponse(
    Guid Id,
    string TransactionNumber,
    string Status);
