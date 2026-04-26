namespace FinLedger.Identity.Domain.Constants;

public static class IdentityRoleNames
{
    public const string SystemAdmin = "SystemAdmin";
    public const string BankAdmin = "BankAdmin";
    public const string BankOperator = "BankOperator";
    public const string CompanyAdmin = "CompanyAdmin";
    public const string CompanyOperator = "CompanyOperator";
    public const string GovernmentAdmin = "GovernmentAdmin";
    public const string Auditor = "Auditor";
    public const string ApiClient = "ApiClient";

    public static readonly string[] All =
    [
        SystemAdmin,
        BankAdmin,
        BankOperator,
        CompanyAdmin,
        CompanyOperator,
        GovernmentAdmin,
        Auditor,
        ApiClient
    ];
}
