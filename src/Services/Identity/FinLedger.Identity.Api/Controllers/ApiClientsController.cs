using FinLedger.Contracts.Identity;
using FinLedger.Identity.Application.Abstractions;
using FinLedger.Identity.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Identity.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/identity/api-clients")]
public sealed class ApiClientsController : ControllerBase
{
    private readonly IApiClientManagementService _apiClientManagementService;

    public ApiClientsController(IApiClientManagementService apiClientManagementService)
    {
        _apiClientManagementService = apiClientManagementService;
    }

    [Authorize(Policy = IdentityPermissionNames.ApiClientsRead)]
    [HttpGet]
    public Task<IReadOnlyCollection<ApiClientDto>> GetApiClients(CancellationToken cancellationToken)
    {
        return _apiClientManagementService.GetApiClientsAsync(cancellationToken);
    }

    [Authorize(Policy = IdentityPermissionNames.ApiClientsWrite)]
    [HttpPost]
    public async Task<ActionResult<ApiClientDto>> CreateApiClient(CreateApiClientRequest request, CancellationToken cancellationToken)
    {
        var result = await _apiClientManagementService.CreateApiClientAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetApiClients), new { clientId = result.Id }, result);
    }

    [Authorize(Policy = IdentityPermissionNames.ApiClientsWrite)]
    [HttpPost("{clientId:guid}/keys")]
    public Task<ApiKeyCreatedResponse> CreateApiKey(Guid clientId, CreateApiKeyRequest request, CancellationToken cancellationToken)
    {
        return _apiClientManagementService.CreateApiKeyAsync(clientId, request, cancellationToken);
    }

    [Authorize(Policy = IdentityPermissionNames.ApiClientsWrite)]
    [HttpPost("{clientId:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid clientId, CancellationToken cancellationToken)
    {
        await _apiClientManagementService.RevokeApiClientAsync(clientId, cancellationToken);
        return NoContent();
    }
}
