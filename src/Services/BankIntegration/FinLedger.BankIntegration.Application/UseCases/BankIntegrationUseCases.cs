using FinLedger.Contracts.BankIntegration;

namespace FinLedger.BankIntegration.Application.UseCases;

public sealed record ValidatePayerBankCommand(
    Guid PayerParticipantId,
    Guid PayerAccountId,
    string PayerBankCode,
    string PayerAccountNumber,
    decimal Amount,
    string CurrencyCode,
    string? ExternalReference);

public sealed record ValidateReceiverBankCommand(
    Guid ReceiverParticipantId,
    Guid ReceiverAccountId,
    string ReceiverBankCode,
    string ReceiverAccountNumber,
    decimal Amount,
    string CurrencyCode,
    string? ExternalReference);

public sealed record BankValidationResult(
    bool IsValid,
    string? ErrorCode,
    string Message,
    Guid ParticipantId,
    Guid AccountId,
    string BankCode,
    string AccountNumber);

public sealed record CreateBankConnectionCommand(Guid ParticipantId, string BankCode, string DisplayName, string BaseUrl, string ApiKey);

public sealed record UpdateBankConnectionCommand(Guid Id, string BankCode, string DisplayName, string BaseUrl, string ApiKey, string Status);

public sealed record GetBankConnectionByParticipantQuery(Guid ParticipantId);

public interface IBankIntegrationUseCaseService
{
    Task<BankValidationResult> ValidatePayerBankAsync(ValidatePayerBankCommand command, CancellationToken cancellationToken = default);

    Task<BankValidationResult> ValidateReceiverBankAsync(ValidateReceiverBankCommand command, CancellationToken cancellationToken = default);

    Task<BankConnectionResponse> CreateBankConnectionAsync(CreateBankConnectionCommand command, CancellationToken cancellationToken = default);

    Task<BankConnectionResponse> UpdateBankConnectionAsync(UpdateBankConnectionCommand command, CancellationToken cancellationToken = default);

    Task<BankConnectionResponse> GetBankConnectionByParticipantAsync(GetBankConnectionByParticipantQuery query, CancellationToken cancellationToken = default);
}
