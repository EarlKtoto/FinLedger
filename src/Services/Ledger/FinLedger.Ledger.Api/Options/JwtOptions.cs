namespace FinLedger.Ledger.Api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "FinLedger.Identity";

    public string Audience { get; set; } = "FinLedger";

    public string SigningKey { get; set; } = "development-only-signing-key-change-this-value";
}
