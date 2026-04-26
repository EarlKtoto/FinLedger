using FinLedger.Identity.Application.Models;
using FinLedger.Identity.Domain.Entities;

namespace FinLedger.Identity.Application.Abstractions;

public interface IJwtTokenFactory
{
    Task<JwtAccessToken> CreateAccessTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}
