using System.ComponentModel.DataAnnotations;

namespace FinLedger.Gateway.Api.Contracts.Requests;

public sealed record CreatePaymentRequest(
    Guid PayerParticipantId,
    Guid PayerAccountId,
    [Required, StringLength(50)] string PayerBankCode,
    [Required, StringLength(100)] string PayerAccountNumber,
    Guid ReceiverParticipantId,
    Guid ReceiverAccountId,
    [Required, StringLength(50)] string ReceiverBankCode,
    [Required, StringLength(100)] string ReceiverAccountNumber,
    decimal Amount,
    [Required, StringLength(3, MinimumLength = 3)] string Currency,
    [StringLength(500)] string? Description,
    [StringLength(100)] string? ExternalReference);
