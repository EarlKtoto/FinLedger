using FinLedger.Contracts.Identity;

namespace FinLedger.Identity.Application.Abstractions;

public interface IUserManagementService
{
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task BlockUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UnblockUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserDto> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);

    Task<UserDto> RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
}
