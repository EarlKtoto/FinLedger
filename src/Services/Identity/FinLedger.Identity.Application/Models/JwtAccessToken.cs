namespace FinLedger.Identity.Application.Models;

public sealed record JwtAccessToken(string Value, DateTimeOffset ExpiresAtUtc);
