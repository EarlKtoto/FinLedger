using FinLedger.Contracts.Identity;
using FinLedger.Identity.Application.Abstractions;
using FinLedger.Identity.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Identity.Api.Controllers;

[ApiController]
[Route("api/identity/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public Task<TokenResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return _authService.LoginAsync(request, GetRequestMetadata(), cancellationToken);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public Task<TokenResponse> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return _authService.RefreshAsync(request, GetRequestMetadata(), cancellationToken);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, GetRequestMetadata(), cancellationToken);
        return NoContent();
    }

    private RequestMetadata GetRequestMetadata()
    {
        return new RequestMetadata(HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString());
    }
}
