namespace FinLedger.Accounts.Application.Constants;

public static class AccountPermissions
{
    public const string Read = "accounts:read";
    public const string Create = "accounts:create";
    public const string Update = "accounts:update";
    public const string Activate = "accounts:activate";
    public const string Suspend = "accounts:suspend";
    public const string Freeze = "accounts:freeze";
    public const string Close = "accounts:close";
    public const string Validate = "accounts:validate";
    public const string AccountLimitsRead = "account-limits:read";
    public const string AccountLimitsManage = "account-limits:manage";

    public static readonly string[] All =
    [
        Read,
        Create,
        Update,
        Activate,
        Suspend,
        Freeze,
        Close,
        Validate,
        AccountLimitsRead,
        AccountLimitsManage
    ];
}
