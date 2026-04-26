using FinLedger.Contracts.Identity;

namespace FinLedger.Identity.Application.Abstractions;

public interface IApiClientManagementService
{
    Task<IReadOnlyCollection<ApiClientDto>> GetApiClientsAsync(CancellationToken cancellationToken = default);

    Task<ApiClientDto> CreateApiClientAsync(CreateApiClientRequest request, CancellationToken cancellationToken = default);

    Task<ApiKeyCreatedResponse> CreateApiKeyAsync(Guid clientId, CreateApiKeyRequest request, CancellationToken cancellationToken = default);

    Task RevokeApiClientAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<ValidateApiKeyResponse> ValidateApiKeyAsync(ValidateApiKeyRequest request, CancellationToken cancellationToken = default);
}
