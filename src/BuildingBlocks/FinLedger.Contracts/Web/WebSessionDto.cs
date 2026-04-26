namespace FinLedger.Contracts.Web;

public sealed record CreateWebSessionRequest(string Reference, string Description, decimal Amount);

public sealed record WebSessionDto(Guid Id, string Reference, string Description, decimal Amount, string Status, DateTimeOffset CreatedAt);
