using FinLedger.BankIntegration.Domain.Enums;

namespace FinLedger.BankIntegration.Domain.Entities;

public sealed class BankIntegrationRequestLog
{
    private BankIntegrationRequestLog()
    {
    }

    private BankIntegrationRequestLog(
        Guid? bankConnectionId,
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
        DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        BankConnectionId = bankConnectionId;
        ParticipantId = participantId;
        ValidationType = validationType;
        RequestUrl = requestUrl;
        RequestPayload = requestPayload;
        ResponsePayload = responsePayload;
        HttpStatusCode = httpStatusCode;
        Succeeded = succeeded;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        DurationMs = durationMs;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid? BankConnectionId { get; private set; }

    public BankConnection? BankConnection { get; private set; }

    public Guid ParticipantId { get; private set; }

    public BankValidationType ValidationType { get; private set; }

    public string RequestUrl { get; private set; } = string.Empty;

    public string RequestPayload { get; private set; } = string.Empty;

    public string? ResponsePayload { get; private set; }

    public int? HttpStatusCode { get; private set; }

    public bool Succeeded { get; private set; }

    public string? ErrorCode { get; private set; }

    public string? ErrorMessage { get; private set; }

    public long DurationMs { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static BankIntegrationRequestLog Create(
        Guid? bankConnectionId,
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
        DateTimeOffset createdAtUtc)
    {
        return new BankIntegrationRequestLog(bankConnectionId, participantId, validationType, requestUrl, requestPayload, responsePayload, httpStatusCode, succeeded, errorCode, errorMessage, durationMs, createdAtUtc);
    }
}
