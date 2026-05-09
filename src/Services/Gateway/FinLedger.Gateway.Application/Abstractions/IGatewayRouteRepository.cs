using FinLedger.Gateway.Domain.Entities;

namespace FinLedger.Gateway.Application.Abstractions;

public interface IGatewayRouteRepository
{
    Task AddAsync(GatewayRoute entity, CancellationToken cancellationToken = default);
    Task<GatewayRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
