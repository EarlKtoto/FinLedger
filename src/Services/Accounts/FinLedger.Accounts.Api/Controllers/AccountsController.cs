using FinLedger.Accounts.Application.Constants;
using FinLedger.Accounts.Application.UseCases;
using FinLedger.Contracts.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Accounts.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly IAccountUseCaseService _accounts;

    public AccountsController(IAccountUseCaseService accounts)
    {
        _accounts = accounts;
    }

    [Authorize(Policy = AccountPermissions.Read)]
    [HttpGet]
    public Task<IReadOnlyCollection<AccountDto>> GetAccounts(CancellationToken cancellationToken)
    {
        return _accounts.GetAccountsAsync(cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Read)]
    [HttpGet("{accountId:guid}")]
    public Task<AccountDto> GetById(Guid accountId, CancellationToken cancellationToken)
    {
        return _accounts.GetAccountByIdAsync(new GetAccountByIdQuery(accountId), cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Read)]
    [HttpGet("by-number/{accountNumber}")]
    public Task<AccountDto> GetByNumber(string accountNumber, CancellationToken cancellationToken)
    {
        return _accounts.GetAccountByNumberAsync(new GetAccountByNumberQuery(accountNumber), cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Read)]
    [HttpGet("participant/{participantId:guid}")]
    public Task<IReadOnlyCollection<AccountDto>> GetByParticipant(Guid participantId, CancellationToken cancellationToken)
    {
        return _accounts.GetAccountsByParticipantAsync(new GetAccountsByParticipantQuery(participantId), cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Create)]
    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _accounts.CreateAccountAsync(
            new CreateAccountCommand(
                request.ParticipantId,
                request.Type,
                request.CurrencyCode,
                request.DisplayName,
                request.AllowIncomingPayments,
                request.AllowOutgoingPayments),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { accountId = result.Id }, result);
    }

    [Authorize(Policy = AccountPermissions.Update)]
    [HttpPut("{accountId:guid}")]
    public Task<AccountDto> Update(Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        return _accounts.UpdateAccountAsync(
            new UpdateAccountCommand(accountId, request.DisplayName, request.AllowIncomingPayments, request.AllowOutgoingPayments),
            cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Activate)]
    [HttpPost("{accountId:guid}/activate")]
    public Task<AccountDto> Activate(Guid accountId, CancellationToken cancellationToken)
    {
        return _accounts.ActivateAccountAsync(new ActivateAccountCommand(accountId), cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Suspend)]
    [HttpPost("{accountId:guid}/suspend")]
    public Task<AccountDto> Suspend(Guid accountId, CancellationToken cancellationToken)
    {
        return _accounts.SuspendAccountAsync(new SuspendAccountCommand(accountId), cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Freeze)]
    [HttpPost("{accountId:guid}/freeze")]
    public Task<AccountDto> Freeze(Guid accountId, CancellationToken cancellationToken)
    {
        return _accounts.FreezeAccountAsync(new FreezeAccountCommand(accountId), cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Close)]
    [HttpPost("{accountId:guid}/close")]
    public Task<AccountDto> Close(Guid accountId, CloseAccountRequest request, CancellationToken cancellationToken)
    {
        return _accounts.CloseAccountAsync(new CloseAccountCommand(accountId, request.Reason), cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Validate)]
    [HttpPost("validate/incoming")]
    public Task<ValidateAccountPaymentResponse> ValidateIncoming(ValidateAccountPaymentRequest request, CancellationToken cancellationToken)
    {
        return _accounts.ValidateIncomingAsync(
            new ValidateAccountForIncomingPaymentQuery(request.AccountId, request.AccountNumber, request.CurrencyCode, request.Amount),
            cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.Validate)]
    [HttpPost("validate/outgoing")]
    public Task<ValidateAccountPaymentResponse> ValidateOutgoing(ValidateAccountPaymentRequest request, CancellationToken cancellationToken)
    {
        return _accounts.ValidateOutgoingAsync(
            new ValidateAccountForOutgoingPaymentQuery(request.AccountId, request.AccountNumber, request.CurrencyCode, request.Amount),
            cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.AccountLimitsRead)]
    [HttpGet("{accountId:guid}/limits")]
    public Task<IReadOnlyCollection<AccountLimitDto>> GetLimits(Guid accountId, CancellationToken cancellationToken)
    {
        return _accounts.GetLimitsAsync(accountId, cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.AccountLimitsManage)]
    [HttpPost("{accountId:guid}/limits")]
    public async Task<ActionResult<AccountLimitDto>> SetLimit(Guid accountId, SetAccountLimitRequest request, CancellationToken cancellationToken)
    {
        var result = await _accounts.SetLimitAsync(new SetAccountLimitCommand(accountId, null, request.LimitType, request.Amount), cancellationToken);
        return CreatedAtAction(nameof(GetLimits), new { accountId }, result);
    }

    [Authorize(Policy = AccountPermissions.AccountLimitsManage)]
    [HttpPut("{accountId:guid}/limits/{limitId:guid}")]
    public Task<AccountLimitDto> UpdateLimit(Guid accountId, Guid limitId, SetAccountLimitRequest request, CancellationToken cancellationToken)
    {
        return _accounts.SetLimitAsync(new SetAccountLimitCommand(accountId, limitId, request.LimitType, request.Amount), cancellationToken);
    }

    [Authorize(Policy = AccountPermissions.AccountLimitsManage)]
    [HttpDelete("{accountId:guid}/limits/{limitId:guid}")]
    public async Task<IActionResult> RemoveLimit(Guid accountId, Guid limitId, CancellationToken cancellationToken)
    {
        await _accounts.RemoveLimitAsync(new RemoveAccountLimitCommand(accountId, limitId), cancellationToken);
        return NoContent();
    }
}
