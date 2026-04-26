namespace FinLedger.Identity.Infrastructure.Options;

public sealed class IdentitySecurityOptions
{
    public const string SectionName = "IdentitySecurity";

    public string SecretPepper { get; set; } = "development-only-secret-pepper-change-this-value";
}
