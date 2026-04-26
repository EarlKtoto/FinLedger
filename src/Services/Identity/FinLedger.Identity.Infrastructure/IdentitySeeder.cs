using System.Security.Claims;
using FinLedger.Identity.Domain.Constants;
using FinLedger.Identity.Domain.Entities;
using FinLedger.Identity.Domain.Enums;
using FinLedger.Identity.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinLedger.Identity.Infrastructure;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<DefaultAdminOptions>>().Value;

        foreach (var roleName in IdentityRoleNames.All)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new IdentityRole<Guid>(roleName);
                Ensure(await roleManager.CreateAsync(role));
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            foreach (var permission in PermissionCatalog.GetPermissionsForRole(roleName))
            {
                if (existingClaims.Any(x => x.Type == IdentityClaimTypes.Permission && x.Value == permission))
                {
                    continue;
                }

                Ensure(await roleManager.AddClaimAsync(role, new Claim(IdentityClaimTypes.Permission, permission)));
            }
        }

        var admin = await userManager.FindByEmailAsync(adminOptions.Email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = adminOptions.Email,
                UserName = adminOptions.UserName,
                FullName = adminOptions.FullName,
                EmailConfirmed = true,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            Ensure(await userManager.CreateAsync(admin, adminOptions.Password));
        }

        if (!await userManager.IsInRoleAsync(admin, IdentityRoleNames.SystemAdmin))
        {
            Ensure(await userManager.AddToRoleAsync(admin, IdentityRoleNames.SystemAdmin));
        }
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
    }
}
