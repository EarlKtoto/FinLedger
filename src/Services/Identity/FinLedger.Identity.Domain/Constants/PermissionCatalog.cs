namespace FinLedger.Identity.Domain.Constants;

public static class PermissionCatalog
{
    public static IReadOnlyCollection<string> GetPermissionsForRole(string roleName)
    {
        return roleName switch
        {
            IdentityRoleNames.SystemAdmin => IdentityPermissionNames.All,
            IdentityRoleNames.BankAdmin =>
            [
                IdentityPermissionNames.UsersRead,
                IdentityPermissionNames.UsersWrite,
                IdentityPermissionNames.RolesManage,
                IdentityPermissionNames.ApiClientsRead,
                IdentityPermissionNames.ApiClientsWrite
            ],
            IdentityRoleNames.BankOperator =>
            [
                IdentityPermissionNames.UsersRead,
                IdentityPermissionNames.ApiClientsRead
            ],
            IdentityRoleNames.CompanyAdmin =>
            [
                IdentityPermissionNames.UsersRead,
                IdentityPermissionNames.UsersWrite,
                IdentityPermissionNames.RolesManage
            ],
            IdentityRoleNames.CompanyOperator => [IdentityPermissionNames.UsersRead],
            IdentityRoleNames.GovernmentAdmin =>
            [
                IdentityPermissionNames.UsersRead,
                IdentityPermissionNames.AuditRead
            ],
            IdentityRoleNames.Auditor =>
            [
                IdentityPermissionNames.UsersRead,
                IdentityPermissionNames.AuditRead
            ],
            IdentityRoleNames.ApiClient => [IdentityPermissionNames.ApiClientsRead],
            _ => []
        };
    }
}
