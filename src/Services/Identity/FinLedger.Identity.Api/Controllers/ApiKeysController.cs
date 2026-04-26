using FinLedger.Contracts.Identity;
using FinLedger.Identity.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Identity.Api.Controllers;

[ApiController]
[Route("api/identity/api-keys")]
public sealed class ApiKeysController : ControllerBase
{
    private readonly IApiClientManagementService _apiClientManagementService;

    public ApiKeysController(IApiClientManagementService apiClientManagementService)
    {
        _apiClientManagementService = apiClientManagementService;
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public Task<ValidateApiKeyResponse> Validate(ValidateApiKeyRequest request, CancellationToken cancellationToken)
    {
        return _apiClientManagementService.ValidateApiKeyAsync(request, cancellationToken);
    }
}
