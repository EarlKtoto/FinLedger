namespace FinLedger.Identity.Infrastructure.Options;

public sealed class DefaultAdminOptions
{
    public const string SectionName = "DefaultAdmin";

    public string Email { get; set; } = "admin@finledger.local";

    public string UserName { get; set; } = "admin@finledger.local";

    public string FullName { get; set; } = "FinLedger System Admin";

    public string Password { get; set; } = "ChangeMe!12345";
}
