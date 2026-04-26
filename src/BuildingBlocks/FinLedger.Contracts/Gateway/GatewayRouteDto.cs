namespace FinLedger.Contracts.Gateway;

public sealed record CreateGatewayRouteRequest(string Reference, string Description, decimal Amount);

public sealed record GatewayRouteDto(Guid Id, string Reference, string Description, decimal Amount, string Status, DateTimeOffset CreatedAt);
