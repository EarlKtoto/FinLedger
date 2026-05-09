using FinLedger.Gateway.Application.Abstractions;
using FinLedger.Gateway.Domain.Entities;
using FinLedger.Gateway.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Gateway.Infrastructure.Repositories;

public sealed class EfGatewayRouteRepository : IGatewayRouteRepository
{
    private readonly GatewayDbContext _dbContext;
    public EfGatewayRouteRepository(GatewayDbContext dbContext) => _dbContext = dbContext;
    public async Task AddAsync(GatewayRoute entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.GatewayRoutes.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public Task<GatewayRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _dbContext.GatewayRoutes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}