using System.Security.Claims;
using FinLedger.Contracts.Identity;
using FinLedger.Identity.Application.Abstractions;
using FinLedger.Identity.Application.Exceptions;
using FinLedger.Identity.Domain.Constants;
using FinLedger.Identity.Domain.Entities;
using FinLedger.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Identity.Infrastructure.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IDateTimeProvider dateTimeProvider)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId);
        return await MapAsync(user);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.OrderBy(x => x.Email).ToListAsync(cancellationToken);
        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            result.Add(await MapAsync(user));
        }

        return result;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var roles = request.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];
        await EnsureRolesExistAsync(roles);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.UserName,
            FullName = request.FullName,
            ParticipantId = request.ParticipantId,
            Status = UserStatus.Active,
            EmailConfirmed = true,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        EnsureIdentityResult(result);

        if (roles.Length > 0)
        {
            EnsureIdentityResult(await _userManager.AddToRolesAsync(user, roles));
        }

        return await MapAsync(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId);
        user.Email = request.Email;
        user.UserName = request.UserName;
        user.FullName = request.FullName;
        user.ParticipantId = request.ParticipantId;
        user.UpdatedAtUtc = _dateTimeProvider.UtcNow;

        EnsureIdentityResult(await _userManager.UpdateAsync(user));
        return await MapAsync(user);
    }

    public async Task BlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId);
        user.Status = UserStatus.Blocked;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.UpdatedAtUtc = _dateTimeProvider.UtcNow;
        EnsureIdentityResult(await _userManager.UpdateAsync(user));
    }

    public async Task UnblockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId);
        user.Status = UserStatus.Active;
        user.LockoutEnd = null;
        user.UpdatedAtUtc = _dateTimeProvider.UtcNow;
        EnsureIdentityResult(await _userManager.UpdateAsync(user));
    }

    public async Task<UserDto> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        await EnsureRolesExistAsync([roleName]);
        var user = await FindUserAsync(userId);
        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            EnsureIdentityResult(await _userManager.AddToRoleAsync(user, roleName));
        }

        return await MapAsync(user);
    }

    public async Task<UserDto> RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId);
        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            EnsureIdentityResult(await _userManager.RemoveFromRoleAsync(user, roleName));
        }

        return await MapAsync(user);
    }

    private async Task<ApplicationUser> FindUserAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new IdentityNotFoundException($"User '{userId}' was not found.");
    }

    private async Task EnsureRolesExistAsync(IEnumerable<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            if (!IdentityRoleNames.All.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            {
                throw new IdentityValidationException($"Role '{roleName}' is not supported.");
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                throw new IdentityValidationException($"Role '{roleName}' does not exist.");
            }
        }
    }

    private async Task<UserDto> MapAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(roles);

        return new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.FullName,
            user.ParticipantId,
            user.Status.ToString(),
            user.CreatedAtUtc,
            user.UpdatedAtUtc,
            user.LastLoginAtUtc,
            roles.ToArray(),
            permissions);
    }

    private async Task<IReadOnlyCollection<string>> GetPermissionsAsync(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in claims.Where(x => x.Type == IdentityClaimTypes.Permission))
                {
                    permissions.Add(claim.Value);
                }
            }

            foreach (var permission in PermissionCatalog.GetPermissionsForRole(roleName))
            {
                permissions.Add(permission);
            }
        }

        return permissions.ToArray();
    }

    private static void EnsureIdentityResult(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new IdentityValidationException(result.Errors.Select(x => x.Description));
        }
    }
}
