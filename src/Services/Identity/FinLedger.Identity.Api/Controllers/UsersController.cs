using FinLedger.Contracts.Identity;
using FinLedger.Identity.Application.Abstractions;
using FinLedger.Identity.Application.Exceptions;
using FinLedger.Identity.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Identity.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/identity/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("me")]
    public Task<UserDto> Me(CancellationToken cancellationToken)
    {
        return _userManagementService.GetCurrentUserAsync(GetCurrentUserId(), cancellationToken);
    }

    [Authorize(Policy = IdentityPermissionNames.UsersRead)]
    [HttpGet]
    public Task<IReadOnlyCollection<UserDto>> GetUsers(CancellationToken cancellationToken)
    {
        return _userManagementService.GetUsersAsync(cancellationToken);
    }

    [Authorize(Policy = IdentityPermissionNames.UsersWrite)]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUsers), new { userId = result.Id }, result);
    }

    [Authorize(Policy = IdentityPermissionNames.UsersWrite)]
    [HttpPut("{userId:guid}")]
    public Task<UserDto> Update(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return _userManagementService.UpdateUserAsync(userId, request, cancellationToken);
    }

    [Authorize(Policy = IdentityPermissionNames.UsersWrite)]
    [HttpPost("{userId:guid}/block")]
    public async Task<IActionResult> Block(Guid userId, CancellationToken cancellationToken)
    {
        await _userManagementService.BlockUserAsync(userId, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = IdentityPermissionNames.UsersWrite)]
    [HttpPost("{userId:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid userId, CancellationToken cancellationToken)
    {
        await _userManagementService.UnblockUserAsync(userId, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = IdentityPermissionNames.RolesManage)]
    [HttpPost("{userId:guid}/roles")]
    public Task<UserDto> AssignRole(Guid userId, AssignRoleRequest request, CancellationToken cancellationToken)
    {
        return _userManagementService.AssignRoleAsync(userId, request.RoleName, cancellationToken);
    }

    [Authorize(Policy = IdentityPermissionNames.RolesManage)]
    [HttpDelete("{userId:guid}/roles/{roleName}")]
    public Task<UserDto> RemoveRole(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        return _userManagementService.RemoveRoleAsync(userId, roleName, cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        var subject = User.FindFirst("sub")?.Value;
        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new IdentityUnauthorizedException("JWT subject is missing or invalid.");
    }
}
