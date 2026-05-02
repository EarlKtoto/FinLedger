using FinLedger.BankIntegration.Domain.Enums;

namespace FinLedger.BankIntegration.Domain.Entities;

public sealed class BankConnection
{
    private BankConnection()
    {
    }

    private BankConnection(Guid participantId, string bankCode, string displayName, string baseUrl, string apiKey, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        ParticipantId = participantId;
        BankCode = NormalizeCode(bankCode);
        DisplayName = displayName.Trim();
        BaseUrl = NormalizeBaseUrl(baseUrl);
        ApiKey = apiKey.Trim();
        Status = BankConnectionStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ParticipantId { get; private set; }

    public string BankCode { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string BaseUrl { get; private set; } = string.Empty;

    public string ApiKey { get; private set; } = string.Empty;

    public BankConnectionStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static BankConnection Create(Guid participantId, string bankCode, string displayName, string baseUrl, string apiKey, DateTimeOffset createdAtUtc)
    {
        if (participantId == Guid.Empty)
        {
            throw new ArgumentException("ParticipantId is required.", nameof(participantId));
        }

        return new BankConnection(participantId, Required(bankCode, nameof(bankCode)), Required(displayName, nameof(displayName)), Required(baseUrl, nameof(baseUrl)), Required(apiKey, nameof(apiKey)), createdAtUtc);
    }

    public void Update(string bankCode, string displayName, string baseUrl, string apiKey, BankConnectionStatus status, DateTimeOffset updatedAtUtc)
    {
        BankCode = NormalizeCode(Required(bankCode, nameof(bankCode)));
        DisplayName = Required(displayName, nameof(displayName));
        BaseUrl = NormalizeBaseUrl(Required(baseUrl, nameof(baseUrl)));
        ApiKey = Required(apiKey, nameof(apiKey));
        Status = status;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        return baseUrl.Trim().TrimEnd('/');
    }
}
