using FinLedger.Contracts.Ledger;
using FinLedger.Ledger.Application.Constants;
using FinLedger.Ledger.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Ledger.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ledger")]
public sealed class LedgerController : ControllerBase
{
    private readonly ILedgerUseCaseService _ledger;

    public LedgerController(ILedgerUseCaseService ledger)
    {
        _ledger = ledger;
    }

    [Authorize(Policy = LedgerPermissions.LedgerAccountsCreate)]
    [HttpPost("accounts")]
    public async Task<ActionResult<LedgerAccountDto>> RegisterAccount(RegisterLedgerAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _ledger.RegisterLedgerAccountAsync(
            new RegisterLedgerAccountCommand(request.AccountId, request.ParticipantId, request.AccountNumber, request.CurrencyCode, request.InitialAvailableBalance),
            cancellationToken);

        return CreatedAtAction(nameof(GetAccount), new { accountId = result.AccountId }, result);
    }

    [Authorize(Policy = LedgerPermissions.LedgerAccountsRead)]
    [HttpGet("accounts/{accountId:guid}")]
    public Task<LedgerAccountDto> GetAccount(Guid accountId, CancellationToken cancellationToken)
    {
        return _ledger.GetLedgerAccountAsync(new GetLedgerAccountQuery(accountId), cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.BalancesRead)]
    [HttpGet("accounts/{accountId:guid}/balance")]
    public Task<AccountBalanceDto> GetBalance(Guid accountId, CancellationToken cancellationToken)
    {
        return _ledger.GetBalanceAsync(new GetBalanceQuery(accountId), cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.FundsReserve)]
    [HttpPost("reservations")]
    public Task<FundsReservationDto> ReserveFunds(ReserveFundsRequest request, CancellationToken cancellationToken)
    {
        return _ledger.ReserveFundsAsync(
            new ReserveFundsCommand(
                request.AccountId,
                request.ExternalTransactionId,
                request.IdempotencyKey,
                request.Amount,
                request.CurrencyCode,
                request.ExpiresAtUtc,
                request.Description),
            cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.FundsRelease)]
    [HttpPost("reservations/{reservationId:guid}/release")]
    public Task<FundsReservationDto> ReleaseReservation(Guid reservationId, ReleaseReservationRequest request, CancellationToken cancellationToken)
    {
        return _ledger.ReleaseReservationAsync(new ReleaseReservationCommand(reservationId, request.IdempotencyKey, request.Description), cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.FundsCapture)]
    [HttpPost("reservations/{reservationId:guid}/capture")]
    public Task<LedgerTransactionDto> CaptureReservation(Guid reservationId, CaptureReservationRequest request, CancellationToken cancellationToken)
    {
        return _ledger.CaptureReservationAsync(
            new CaptureReservationCommand(
                reservationId,
                request.CreditAccountId,
                request.IdempotencyKey,
                request.ExternalTransactionId,
                request.Description),
            cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.TransfersCreate)]
    [HttpPost("transfers")]
    public Task<LedgerTransactionDto> CreateTransfer(CreateTransferRequest request, CancellationToken cancellationToken)
    {
        return _ledger.CreateTransferAsync(
            new CreateTransferCommand(
                request.DebitAccountId,
                request.CreditAccountId,
                request.ExternalTransactionId,
                request.IdempotencyKey,
                request.Amount,
                request.CurrencyCode,
                request.Description),
            cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.LedgerTransactionsReverse)]
    [HttpPost("transactions/{transactionId:guid}/reverse")]
    public Task<LedgerTransactionDto> ReverseTransaction(Guid transactionId, ReverseLedgerTransactionRequest request, CancellationToken cancellationToken)
    {
        return _ledger.ReverseLedgerTransactionAsync(
            new ReverseLedgerTransactionCommand(transactionId, request.IdempotencyKey, request.ExternalTransactionId, request.Description),
            cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.LedgerTransactionsRead)]
    [HttpGet("transactions/{transactionId:guid}")]
    public Task<LedgerTransactionDto> GetTransaction(Guid transactionId, CancellationToken cancellationToken)
    {
        return _ledger.GetLedgerTransactionAsync(new GetLedgerTransactionQuery(transactionId), cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.LedgerTransactionsRead)]
    [HttpGet("transactions/by-external/{externalTransactionId}")]
    public Task<LedgerTransactionDto> GetTransactionByExternalId(string externalTransactionId, CancellationToken cancellationToken)
    {
        return _ledger.GetLedgerTransactionByExternalIdAsync(new GetLedgerTransactionByExternalIdQuery(externalTransactionId), cancellationToken);
    }

    [Authorize(Policy = LedgerPermissions.LedgerTransactionsRead)]
    [HttpGet("accounts/{accountId:guid}/entries")]
    public Task<IReadOnlyCollection<LedgerEntryDto>> GetAccountEntries(Guid accountId, [FromQuery] int skip = 0, [FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        return _ledger.GetAccountEntriesAsync(new GetAccountEntriesQuery(accountId, skip, take), cancellationToken);
    }
}
