using System.ComponentModel.DataAnnotations;

namespace FinLedger.Gateway.Api.Contracts.Requests;

public sealed record CancelPaymentRequest([StringLength(500)] string? Reason);
