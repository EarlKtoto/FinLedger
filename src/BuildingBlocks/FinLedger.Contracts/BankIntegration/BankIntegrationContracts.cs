using System.ComponentModel.DataAnnotations;

namespace FinLedger.Contracts.BankIntegration;

public sealed record ValidatePayerBankRequest(
    Guid PayerParticipantId,
    Guid PayerAccountId,
    [Required] string PayerBankCode,
    [Required] string PayerAccountNumber,
    decimal Amount,
    [Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    string? ExternalReference);

public sealed record ValidateReceiverBankRequest(
    Guid ReceiverParticipantId,
    Guid ReceiverAccountId,
    [Required] string ReceiverBankCode,
    [Required] string ReceiverAccountNumber,
    decimal Amount,
    [Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    string? ExternalReference);

public sealed record BankValidationResponse(
    bool IsValid,
    string? ErrorCode,
    string Message,
    Guid ParticipantId,
    Guid AccountId,
    string BankCode,
    string AccountNumber);

public sealed record CreateBankConnectionRequest(
    Guid ParticipantId,
    [Required] string BankCode,
    [Required] string DisplayName,
    [Required] string BaseUrl,
    [Required] string ApiKey);

public sealed record UpdateBankConnectionRequest(
    [Required] string BankCode,
    [Required] string DisplayName,
    [Required] string BaseUrl,
    [Required] string ApiKey,
    [Required] string Status);

public sealed record BankConnectionResponse(
    Guid Id,
    Guid ParticipantId,
    string BankCode,
    string DisplayName,
    string BaseUrl,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record ExternalBankValidationRequest(
    Guid ParticipantId,
    Guid AccountId,
    string BankCode,
    string AccountNumber,
    decimal Amount,
    string CurrencyCode,
    string ValidationType,
    string? ExternalReference);

public sealed record ExternalBankValidationResponse(
    bool IsValid,
    string? ErrorCode,
    string? Message);

public sealed record RegisterBankConnectionRequest(string BankCode, string DisplayName, string BaseUrl);

public sealed record BankConnectionDto(Guid Id, string BankCode, string DisplayName, string BaseUrl, string Status, DateTimeOffset CreatedAt);

public sealed record BankVerificationRequestDto(string BankCode, string AccountNumber);

public sealed record BankVerificationResultDto(string BankCode, string AccountNumber, bool IsAvailable, string Message);
