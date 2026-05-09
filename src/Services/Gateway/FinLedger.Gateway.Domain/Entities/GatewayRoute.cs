namespace FinLedger.Gateway.Domain.Entities;

public sealed class GatewayRoute
{
    private GatewayRoute() { }
    private GatewayRoute(string reference, string description, decimal amount)
    {
        Id = Guid.NewGuid(); Reference = reference; Description = description; Amount = amount; Status = "Draft"; CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public static GatewayRoute Create(string reference, string description, decimal amount) => new(reference, description, amount);
    public void MarkCompleted() => Status = "Completed";
    public void MarkFailed() => Status = "Failed";
}