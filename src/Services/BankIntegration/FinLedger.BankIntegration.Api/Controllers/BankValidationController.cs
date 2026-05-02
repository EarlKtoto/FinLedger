using FinLedger.BankIntegration.Application.UseCases;
using FinLedger.Contracts.BankIntegration;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.BankIntegration.Api.Controllers;

[ApiController]
[Route("api/bank-validation")]
public sealed class BankValidationController : ControllerBase
{
    private readonly IBankIntegrationUseCaseService _bankIntegration;

    public BankValidationController(IBankIntegrationUseCaseService bankIntegration)
    {
        _bankIntegration = bankIntegration;
    }

    [HttpPost("payer")]
    public async Task<ActionResult<BankValidationResponse>> ValidatePayer(ValidatePayerBankRequest request, CancellationToken cancellationToken)
    {
        var result = await _bankIntegration.ValidatePayerBankAsync(
            new ValidatePayerBankCommand(
                request.PayerParticipantId,
                request.PayerAccountId,
                request.PayerBankCode,
                request.PayerAccountNumber,
                request.Amount,
                request.CurrencyCode,
                request.ExternalReference),
            cancellationToken);

        return Ok(Map(result));
    }

    [HttpPost("receiver")]
    public async Task<ActionResult<BankValidationResponse>> ValidateReceiver(ValidateReceiverBankRequest request, CancellationToken cancellationToken)
    {
        var result = await _bankIntegration.ValidateReceiverBankAsync(
            new ValidateReceiverBankCommand(
                request.ReceiverParticipantId,
                request.ReceiverAccountId,
                request.ReceiverBankCode,
                request.ReceiverAccountNumber,
                request.Amount,
                request.CurrencyCode,
                request.ExternalReference),
            cancellationToken);

        return Ok(Map(result));
    }

    private static BankValidationResponse Map(BankValidationResult result)
    {
        return new BankValidationResponse(result.IsValid, result.ErrorCode, result.Message, result.ParticipantId, result.AccountId, result.BankCode, result.AccountNumber);
    }
}
