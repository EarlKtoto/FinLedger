namespace FinLedger.Gateway.Api.Contracts.Requests;

public sealed record CancelTransactionClientRequest(string? Reason);
