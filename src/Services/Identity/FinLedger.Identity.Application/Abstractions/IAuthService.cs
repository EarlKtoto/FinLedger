using FinLedger.Contracts.Identity;
using FinLedger.Identity.Application.Models;

namespace FinLedger.Identity.Application.Abstractions;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, RequestMetadata metadata, CancellationToken cancellationToken = default);

    Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, RequestMetadata metadata, CancellationToken cancellationToken = default);

    Task LogoutAsync(LogoutRequest request, RequestMetadata metadata, CancellationToken cancellationToken = default);
}
