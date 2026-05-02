using FinLedger.BankIntegration.Domain.Entities;
using FinLedger.Contracts.BankIntegration;

namespace FinLedger.BankIntegration.Application.Abstractions;

public interface IExternalBankClient
{
    Task<ExternalBankCallResult> ValidateAsync(BankConnection connection, ExternalBankValidationRequest request, CancellationToken cancellationToken = default);
}

public sealed record ExternalBankCallResult(
    bool IsValid,
    string? ErrorCode,
    string Message,
    string RequestUrl,
    string RequestPayload,
    string? ResponsePayload,
    int? HttpStatusCode,
    long DurationMs);
