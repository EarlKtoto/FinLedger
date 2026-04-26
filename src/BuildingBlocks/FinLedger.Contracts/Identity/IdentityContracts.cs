using System.ComponentModel.DataAnnotations;

namespace FinLedger.Contracts.Identity;

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record RefreshTokenRequest([Required] string RefreshToken);

public sealed record LogoutRequest([Required] string RefreshToken);

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    DateTimeOffset RefreshTokenExpiresAtUtc);

public sealed record UserDto(
    Guid Id,
    string Email,
    string UserName,
    string FullName,
    Guid? ParticipantId,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? LastLoginAtUtc,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required] string UserName,
    [Required] string FullName,
    Guid? ParticipantId,
    [Required] string Password,
    IReadOnlyCollection<string>? Roles);

public sealed record UpdateUserRequest(
    [Required] string Email,
    [Required] string UserName,
    [Required] string FullName,
    Guid? ParticipantId);

public sealed record AssignRoleRequest([Required] string RoleName);

public sealed record ApiClientDto(
    Guid Id,
    string ClientId,
    string Name,
    Guid? ParticipantId,
    string? Description,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? RevokedAtUtc);

public sealed record CreateApiClientRequest(
    [Required] string Name,
    Guid? ParticipantId,
    string? Description);

public sealed record CreateApiKeyRequest(
    [Required] string Name,
    DateTimeOffset? ExpiresAtUtc);

public sealed record ApiKeyCreatedResponse(
    Guid Id,
    Guid ApiClientId,
    string Name,
    string KeyPrefix,
    string ApiKey,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc);

public sealed record ValidateApiKeyRequest([Required] string ApiKey);

public sealed record ValidateApiKeyResponse(
    bool IsValid,
    Guid? ApiClientId,
    string? ClientId,
    Guid? ParticipantId,
    IReadOnlyCollection<string> Permissions);
