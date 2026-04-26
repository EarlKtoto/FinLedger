namespace FinLedger.Identity.Domain.Constants;

public static class IdentityPermissionNames
{
    public const string UsersRead = "identity.users.read";
    public const string UsersWrite = "identity.users.write";
    public const string RolesManage = "identity.roles.manage";
    public const string ApiClientsRead = "identity.api_clients.read";
    public const string ApiClientsWrite = "identity.api_clients.write";
    public const string AuditRead = "identity.audit.read";

    public static readonly string[] All =
    [
        UsersRead,
        UsersWrite,
        RolesManage,
        ApiClientsRead,
        ApiClientsWrite,
        AuditRead
    ];
}
