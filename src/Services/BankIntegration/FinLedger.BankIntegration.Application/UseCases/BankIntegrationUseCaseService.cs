using System.Text.Json;
using FinLedger.BankIntegration.Application.Abstractions;
using FinLedger.BankIntegration.Application.Constants;
using FinLedger.BankIntegration.Application.Exceptions;
using FinLedger.BankIntegration.Domain.Entities;
using FinLedger.BankIntegration.Domain.Enums;
using FinLedger.Contracts.BankIntegration;

namespace FinLedger.BankIntegration.Application.UseCases;

public sealed class BankIntegrationUseCaseService : IBankIntegrationUseCaseService
{
    private readonly IBankConnectionRepository _connections;
    private readonly IBankIntegrationRequestLogRepository _logs;
    private readonly IExternalBankClient _externalBankClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BankIntegrationUseCaseService(
        IBankConnectionRepository connections,
        IBankIntegrationRequestLogRepository logs,
        IExternalBankClient externalBankClient,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _connections = connections;
        _logs = logs;
        _externalBankClient = externalBankClient;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<BankValidationResult> ValidatePayerBankAsync(ValidatePayerBankCommand command, CancellationToken cancellationToken = default)
    {
        ValidateBankRequest(command.PayerParticipantId, command.PayerAccountId, command.PayerBankCode, command.PayerAccountNumber, command.Amount, command.CurrencyCode);
        return ValidateAsync(
            command.PayerParticipantId,
            command.PayerAccountId,
            command.PayerBankCode,
            command.PayerAccountNumber,
            command.Amount,
            command.CurrencyCode,
            command.ExternalReference,
            BankValidationType.Payer,
            cancellationToken);
    }

    public Task<BankValidationResult> ValidateReceiverBankAsync(ValidateReceiverBankCommand command, CancellationToken cancellationToken = default)
    {
        ValidateBankRequest(command.ReceiverParticipantId, command.ReceiverAccountId, command.ReceiverBankCode, command.ReceiverAccountNumber, command.Amount, command.CurrencyCode);
        return ValidateAsync(
            command.ReceiverParticipantId,
            command.ReceiverAccountId,
            command.ReceiverBankCode,
            command.ReceiverAccountNumber,
            command.Amount,
            command.CurrencyCode,
            command.ExternalReference,
            BankValidationType.Receiver,
            cancellationToken);
    }

    public async Task<BankConnectionResponse> CreateBankConnectionAsync(CreateBankConnectionCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _connections.GetByParticipantIdAsync(command.ParticipantId, cancellationToken);
        if (existing is not null)
        {
            throw new BankConnectionConflictException($"Bank connection for participant '{command.ParticipantId}' already exists.");
        }

        var connection = CreateDomain(() => BankConnection.Create(command.ParticipantId, command.BankCode, command.DisplayName, command.BaseUrl, command.ApiKey, _dateTimeProvider.UtcNow));
        await _connections.AddAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(connection);
    }

    public async Task<BankConnectionResponse> UpdateBankConnectionAsync(UpdateBankConnectionCommand command, CancellationToken cancellationToken = default)
    {
        var connection = await _connections.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new BankConnectionNotFoundException($"Bank connection '{command.Id}' was not found.");
        var status = ParseStatus(command.Status);
        ExecuteDomain(() => connection.Update(command.BankCode, command.DisplayName, command.BaseUrl, command.ApiKey, status, _dateTimeProvider.UtcNow));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(connection);
    }

    public async Task<BankConnectionResponse> GetBankConnectionByParticipantAsync(GetBankConnectionByParticipantQuery query, CancellationToken cancellationToken = default)
    {
        var connection = await _connections.GetByParticipantIdAsync(query.ParticipantId, cancellationToken)
            ?? throw new BankConnectionNotFoundException($"Bank connection for participant '{query.ParticipantId}' was not found.");
        return Map(connection);
    }

    private async Task<BankValidationResult> ValidateAsync(
        Guid participantId,
        Guid accountId,
        string bankCode,
        string accountNumber,
        decimal amount,
        string currencyCode,
        string? externalReference,
        BankValidationType validationType,
        CancellationToken cancellationToken)
    {
        var connection = await _connections.GetByParticipantIdAsync(participantId, cancellationToken);
        if (connection is null)
        {
            await LogLocalFailureAsync(null, participantId, validationType, BankIntegrationErrorCodes.BankConnectionNotFound, "Bank connection was not found.", cancellationToken);
            return Failure(participantId, accountId, bankCode, accountNumber, BankIntegrationErrorCodes.BankConnectionNotFound, "Bank connection was not found.");
        }

        if (connection.Status != BankConnectionStatus.Active)
        {
            await LogLocalFailureAsync(connection.Id, participantId, validationType, BankIntegrationErrorCodes.BankConnectionInactive, "Bank connection is not active.", cancellationToken);
            return Failure(participantId, accountId, bankCode, accountNumber, BankIntegrationErrorCodes.BankConnectionInactive, "Bank connection is not active.");
        }

        var request = new ExternalBankValidationRequest(
            participantId,
            accountId,
            bankCode.Trim().ToUpperInvariant(),
            accountNumber.Trim(),
            amount,
            currencyCode.Trim().ToUpperInvariant(),
            validationType.ToString(),
            externalReference);

        var externalResult = await _externalBankClient.ValidateAsync(connection, request, cancellationToken);
        var errorCode = NormalizeBankErrorCode(externalResult.ErrorCode, externalResult.IsValid);
        await AddLogAsync(
            connection.Id,
            participantId,
            validationType,
            externalResult.RequestUrl,
            externalResult.RequestPayload,
            externalResult.ResponsePayload,
            externalResult.HttpStatusCode,
            externalResult.IsValid,
            errorCode,
            externalResult.IsValid ? null : externalResult.Message,
            externalResult.DurationMs,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return externalResult.IsValid
            ? new BankValidationResult(true, null, externalResult.Message, participantId, accountId, request.BankCode, request.AccountNumber)
            : Failure(participantId, accountId, request.BankCode, request.AccountNumber, errorCode, externalResult.Message);
    }

    private async Task LogLocalFailureAsync(Guid? connectionId, Guid participantId, BankValidationType validationType, string errorCode, string message, CancellationToken cancellationToken)
    {
        var requestPayload = JsonSerializer.Serialize(new { participantId, validationType });
        await AddLogAsync(connectionId, participantId, validationType, string.Empty, requestPayload, null, null, false, errorCode, message, 0, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task AddLogAsync(
        Guid? connectionId,
        Guid participantId,
        BankValidationType validationType,
        string requestUrl,
        string requestPayload,
        string? responsePayload,
        int? httpStatusCode,
        bool succeeded,
        string? errorCode,
        string? errorMessage,
        long durationMs,
        CancellationToken cancellationToken)
    {
        var log = BankIntegrationRequestLog.Create(
            connectionId,
            participantId,
            validationType,
            requestUrl,
            requestPayload,
            responsePayload,
            httpStatusCode,
            succeeded,
            errorCode,
            errorMessage,
            durationMs,
            _dateTimeProvider.UtcNow);

        await _logs.AddAsync(log, cancellationToken);
    }

    private static void ValidateBankRequest(Guid participantId, Guid accountId, string bankCode, string accountNumber, decimal amount, string currencyCode)
    {
        if (participantId == Guid.Empty)
        {
            throw new BankIntegrationValidationException("ParticipantId is required.");
        }

        if (accountId == Guid.Empty)
        {
            throw new BankIntegrationValidationException("AccountId is required.");
        }

        if (string.IsNullOrWhiteSpace(bankCode))
        {
            throw new BankIntegrationValidationException("BankCode is required.");
        }

        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new BankIntegrationValidationException("AccountNumber is required.");
        }

        if (amount <= 0)
        {
            throw new BankIntegrationValidationException("Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
        {
            throw new BankIntegrationValidationException("CurrencyCode must contain exactly three characters.");
        }
    }

    private static BankConnectionStatus ParseStatus(string value)
    {
        if (Enum.TryParse<BankConnectionStatus>(value, ignoreCase: true, out var parsed) && Enum.IsDefined(typeof(BankConnectionStatus), parsed))
        {
            return parsed;
        }

        throw new BankIntegrationValidationException($"'{value}' is not a valid bank connection status.");
    }

    private static string NormalizeBankErrorCode(string? errorCode, bool isValid)
    {
        if (isValid)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return BankIntegrationErrorCodes.UnknownBankError;
        }

        var normalized = errorCode.Trim().ToUpperInvariant();
        return BankIntegrationErrorCodes.BankProvidedCodes.Contains(normalized) || normalized.StartsWith("BANK_", StringComparison.Ordinal)
            ? normalized
            : BankIntegrationErrorCodes.UnknownBankError;
    }

    private static BankValidationResult Failure(Guid participantId, Guid accountId, string bankCode, string accountNumber, string? errorCode, string message)
    {
        return new BankValidationResult(false, errorCode, message, participantId, accountId, bankCode.Trim().ToUpperInvariant(), accountNumber.Trim());
    }

    private static T CreateDomain<T>(Func<T> factory)
    {
        try
        {
            return factory();
        }
        catch (ArgumentException exception)
        {
            throw new BankIntegrationValidationException(exception.Message);
        }
    }

    private static void ExecuteDomain(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException exception)
        {
            throw new BankIntegrationValidationException(exception.Message);
        }
    }

    private static BankConnectionResponse Map(BankConnection connection)
    {
        return new BankConnectionResponse(
            connection.Id,
            connection.ParticipantId,
            connection.BankCode,
            connection.DisplayName,
            connection.BaseUrl,
            connection.Status.ToString(),
            connection.CreatedAtUtc,
            connection.UpdatedAtUtc);
    }
}
