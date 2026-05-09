namespace FinLedger.Gateway.Api.Options;

public sealed class ServiceUrlOptions
{
    public const string SectionName = "ServiceUrls";

    public string Transactions { get; set; } = "https://localhost:7004";
}
