using FinLedger.Contracts.Identity;
using FinLedger.Identity.Application.Abstractions;
using FinLedger.Identity.Application.Exceptions;
using FinLedger.Identity.Domain.Constants;
using FinLedger.Identity.Domain.Entities;
using FinLedger.Identity.Domain.Enums;
using FinLedger.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Identity.Infrastructure.Services;

public sealed class ApiClientManagementService : IApiClientManagementService
{
    private readonly IdentityDbContext _dbContext;
    private readonly ISecretGenerator _secretGenerator;
    private readonly ISecretHasher _secretHasher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApiClientManagementService(
        IdentityDbContext dbContext,
        ISecretGenerator secretGenerator,
        ISecretHasher secretHasher,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _secretGenerator = secretGenerator;
        _secretHasher = secretHasher;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyCollection<ApiClientDto>> GetApiClientsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiClients
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<ApiClientDto> CreateApiClientAsync(CreateApiClientRequest request, CancellationToken cancellationToken = default)
    {
        var apiClient = new ApiClient
        {
            Id = Guid.NewGuid(),
            ClientId = $"client_{Guid.NewGuid():N}",
            Name = request.Name,
            ParticipantId = request.ParticipantId,
            Description = request.Description,
            Status = ApiClientStatus.Active,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        _dbContext.ApiClients.Add(apiClient);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(apiClient);
    }

    public async Task<ApiKeyCreatedResponse> CreateApiKeyAsync(Guid clientId, CreateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        var apiClient = await FindClientAsync(clientId, cancellationToken);
        if (apiClient.Status != ApiClientStatus.Active)
        {
            throw new IdentityConflictException("API client is not active.");
        }

        var rawApiKey = _secretGenerator.CreateApiKey(apiClient.ClientId, out var keyPrefix);
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            ApiClientId = apiClient.Id,
            Name = request.Name,
            KeyPrefix = keyPrefix,
            KeyHash = _secretHasher.Hash(rawApiKey),
            ExpiresAtUtc = request.ExpiresAtUtc,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        _dbContext.ApiKeys.Add(apiKey);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiKeyCreatedResponse(apiKey.Id, apiKey.ApiClientId, apiKey.Name, apiKey.KeyPrefix, rawApiKey, apiKey.CreatedAtUtc, apiKey.ExpiresAtUtc);
    }

    public async Task RevokeApiClientAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var apiClient = await _dbContext.ApiClients
            .Include(x => x.ApiKeys)
            .FirstOrDefaultAsync(x => x.Id == clientId, cancellationToken)
            ?? throw new IdentityNotFoundException($"API client '{clientId}' was not found.");

        apiClient.Status = ApiClientStatus.Revoked;
        apiClient.RevokedAtUtc = _dateTimeProvider.UtcNow;
        apiClient.UpdatedAtUtc = _dateTimeProvider.UtcNow;

        foreach (var key in apiClient.ApiKeys.Where(x => !x.IsRevoked))
        {
            key.RevokedAtUtc = _dateTimeProvider.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ValidateApiKeyResponse> ValidateApiKeyAsync(ValidateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        var hash = _secretHasher.Hash(request.ApiKey);
        var apiKey = await _dbContext.ApiKeys
            .Include(x => x.ApiClient)
            .FirstOrDefaultAsync(x => x.KeyHash == hash, cancellationToken);

        if (apiKey?.ApiClient is null || apiKey.ApiClient.Status != ApiClientStatus.Active || !apiKey.IsActive(_dateTimeProvider.UtcNow))
        {
            return new ValidateApiKeyResponse(false, null, null, null, []);
        }

        apiKey.LastUsedAtUtc = _dateTimeProvider.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ValidateApiKeyResponse(
            true,
            apiKey.ApiClient.Id,
            apiKey.ApiClient.ClientId,
            apiKey.ApiClient.ParticipantId,
            PermissionCatalog.GetPermissionsForRole(IdentityRoleNames.ApiClient));
    }

    private async Task<ApiClient> FindClientAsync(Guid clientId, CancellationToken cancellationToken)
    {
        return await _dbContext.ApiClients.FirstOrDefaultAsync(x => x.Id == clientId, cancellationToken)
            ?? throw new IdentityNotFoundException($"API client '{clientId}' was not found.");
    }

    private static ApiClientDto Map(ApiClient apiClient)
    {
        return new ApiClientDto(
            apiClient.Id,
            apiClient.ClientId,
            apiClient.Name,
            apiClient.ParticipantId,
            apiClient.Description,
            apiClient.Status.ToString(),
            apiClient.CreatedAtUtc,
            apiClient.UpdatedAtUtc,
            apiClient.RevokedAtUtc);
    }
}
