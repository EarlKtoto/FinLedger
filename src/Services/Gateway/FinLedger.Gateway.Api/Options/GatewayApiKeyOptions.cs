namespace FinLedger.Gateway.Api.Options;

public sealed class GatewayApiKeyOptions
{
    public const string SectionName = "GatewayApiKeys";

    public string[] Keys { get; set; } = [];
}
