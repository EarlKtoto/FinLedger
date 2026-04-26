using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinLedger.Identity.Application.Abstractions;
using FinLedger.Identity.Application.Models;
using FinLedger.Identity.Domain.Constants;
using FinLedger.Identity.Domain.Entities;
using FinLedger.Identity.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinLedger.Identity.Infrastructure.Services;

public sealed class JwtTokenFactory : IJwtTokenFactory
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtTokenFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<JwtOptions> options,
        IDateTimeProvider dateTimeProvider)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<JwtAccessToken> CreateAccessTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(roles);
        var expiresAt = _dateTimeProvider.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(IdentityClaimTypes.TokenType, "access")
        };

        if (user.ParticipantId.HasValue)
        {
            claims.Add(new Claim(IdentityClaimTypes.ParticipantId, user.ParticipantId.Value.ToString()));
        }

        claims.AddRange(roles.Select(role => new Claim("roles", role)));
        claims.AddRange(permissions.Select(permission => new Claim(IdentityClaimTypes.Permission, permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _dateTimeProvider.UtcNow.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private async Task<IReadOnlyCollection<string>> GetPermissionsAsync(IEnumerable<string> roleNames)
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims.Where(x => x.Type == IdentityClaimTypes.Permission))
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
}
