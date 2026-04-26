using FinLedger.Contracts.Identity;
using FinLedger.Identity.Application.Abstractions;
using FinLedger.Identity.Application.Exceptions;
using FinLedger.Identity.Application.Models;
using FinLedger.Identity.Domain.Entities;
using FinLedger.Identity.Domain.Enums;
using FinLedger.Identity.Infrastructure.Options;
using FinLedger.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinLedger.Identity.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _dbContext;
    private readonly IJwtTokenFactory _jwtTokenFactory;
    private readonly ISecretGenerator _secretGenerator;
    private readonly ISecretHasher _secretHasher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IdentityDbContext dbContext,
        IJwtTokenFactory jwtTokenFactory,
        ISecretGenerator secretGenerator,
        ISecretHasher secretHasher,
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtTokenFactory = jwtTokenFactory;
        _secretGenerator = secretGenerator;
        _secretHasher = secretHasher;
        _dateTimeProvider = dateTimeProvider;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, RequestMetadata metadata, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            await AddLoginAuditAsync(null, request.Email, false, "User not found", metadata, cancellationToken);
            throw new IdentityUnauthorizedException();
        }

        if (user.Status != UserStatus.Active)
        {
            await AddLoginAuditAsync(user.Id, request.Email, false, $"User status is {user.Status}", metadata, cancellationToken);
            throw new IdentityForbiddenException("User account is not active.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await AddLoginAuditAsync(user.Id, request.Email, false, "Invalid password", metadata, cancellationToken);
            throw new IdentityUnauthorizedException();
        }

        user.LastLoginAtUtc = _dateTimeProvider.UtcNow;
        user.UpdatedAtUtc = _dateTimeProvider.UtcNow;
        await _userManager.UpdateAsync(user);
        await AddLoginAuditAsync(user.Id, request.Email, true, null, metadata, cancellationToken);

        return await CreateTokenResponseAsync(user, metadata, cancellationToken);
    }

    public async Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, RequestMetadata metadata, CancellationToken cancellationToken = default)
    {
        var tokenHash = _secretHasher.Hash(request.RefreshToken);
        var refreshToken = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (refreshToken?.User is null || !refreshToken.IsActive(_dateTimeProvider.UtcNow))
        {
            throw new IdentityUnauthorizedException("Refresh token is invalid.");
        }

        if (refreshToken.User.Status != UserStatus.Active)
        {
            throw new IdentityForbiddenException("User account is not active.");
        }

        var newRefreshToken = _secretGenerator.CreateRefreshToken();
        var newRefreshTokenHash = _secretHasher.Hash(newRefreshToken);
        var expiresAt = _dateTimeProvider.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        refreshToken.RevokedAtUtc = _dateTimeProvider.UtcNow;
        refreshToken.RevokedByIp = metadata.IpAddress;
        refreshToken.ReplacedByTokenHash = newRefreshTokenHash;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = refreshToken.UserId,
            TokenHash = newRefreshTokenHash,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            ExpiresAtUtc = expiresAt,
            CreatedByIp = metadata.IpAddress
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = await _jwtTokenFactory.CreateAccessTokenAsync(refreshToken.User, cancellationToken);
        return new TokenResponse(accessToken.Value, newRefreshToken, accessToken.ExpiresAtUtc, expiresAt);
    }

    public async Task LogoutAsync(LogoutRequest request, RequestMetadata metadata, CancellationToken cancellationToken = default)
    {
        var tokenHash = _secretHasher.Hash(request.RefreshToken);
        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        if (refreshToken is null || refreshToken.IsRevoked)
        {
            return;
        }

        refreshToken.RevokedAtUtc = _dateTimeProvider.UtcNow;
        refreshToken.RevokedByIp = metadata.IpAddress;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TokenResponse> CreateTokenResponseAsync(ApplicationUser user, RequestMetadata metadata, CancellationToken cancellationToken)
    {
        var accessToken = await _jwtTokenFactory.CreateAccessTokenAsync(user, cancellationToken);
        var refreshToken = _secretGenerator.CreateRefreshToken();
        var refreshTokenExpiresAt = _dateTimeProvider.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _secretHasher.Hash(refreshToken),
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            ExpiresAtUtc = refreshTokenExpiresAt,
            CreatedByIp = metadata.IpAddress
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokenResponse(accessToken.Value, refreshToken, accessToken.ExpiresAtUtc, refreshTokenExpiresAt);
    }

    private async Task AddLoginAuditAsync(Guid? userId, string email, bool succeeded, string? failureReason, RequestMetadata metadata, CancellationToken cancellationToken)
    {
        _dbContext.LoginAudits.Add(new LoginAudit
        {
            UserId = userId,
            Email = email,
            Succeeded = succeeded,
            FailureReason = failureReason,
            IpAddress = metadata.IpAddress,
            UserAgent = metadata.UserAgent,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
