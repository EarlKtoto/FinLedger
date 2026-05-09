using FinLedger.Gateway.Application.Abstractions;
using FinLedger.Gateway.Domain.Entities;
using FinLedger.Contracts.Gateway;

namespace FinLedger.Gateway.Application.UseCases;

public interface ICreateGatewayRouteUseCase
{
    Task<GatewayRouteDto> ExecuteAsync(CreateGatewayRouteRequest request, CancellationToken cancellationToken = default);
}

public sealed class CreateGatewayRouteUseCase : ICreateGatewayRouteUseCase
{
    private readonly IGatewayRouteRepository _repository;
    public CreateGatewayRouteUseCase(IGatewayRouteRepository repository) => _repository = repository;

    public async Task<GatewayRouteDto> ExecuteAsync(CreateGatewayRouteRequest request, CancellationToken cancellationToken = default)
    {
        var entity = GatewayRoute.Create(request.Reference, request.Description, request.Amount);
        await _repository.AddAsync(entity, cancellationToken);
        return new GatewayRouteDto(entity.Id, entity.Reference, entity.Description, entity.Amount, entity.Status, entity.CreatedAt);
    }
}