using FinLedger.BankIntegration.Application.UseCases;
using FinLedger.Contracts.BankIntegration;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.BankIntegration.Api.Controllers;

[ApiController]
[Route("api/bank-connections")]
public sealed class BankConnectionsController : ControllerBase
{
    private readonly IBankIntegrationUseCaseService _bankIntegration;

    public BankConnectionsController(IBankIntegrationUseCaseService bankIntegration)
    {
        _bankIntegration = bankIntegration;
    }

    [HttpPost]
    public async Task<ActionResult<BankConnectionResponse>> Create(CreateBankConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _bankIntegration.CreateBankConnectionAsync(
            new CreateBankConnectionCommand(request.ParticipantId, request.BankCode, request.DisplayName, request.BaseUrl, request.ApiKey),
            cancellationToken);

        return CreatedAtAction(nameof(GetByParticipant), new { participantId = result.ParticipantId }, result);
    }

    [HttpGet("{participantId:guid}")]
    public Task<BankConnectionResponse> GetByParticipant(Guid participantId, CancellationToken cancellationToken)
    {
        return _bankIntegration.GetBankConnectionByParticipantAsync(new GetBankConnectionByParticipantQuery(participantId), cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<BankConnectionResponse> Update(Guid id, UpdateBankConnectionRequest request, CancellationToken cancellationToken)
    {
        return _bankIntegration.UpdateBankConnectionAsync(
            new UpdateBankConnectionCommand(id, request.BankCode, request.DisplayName, request.BaseUrl, request.ApiKey, request.Status),
            cancellationToken);
    }
}
