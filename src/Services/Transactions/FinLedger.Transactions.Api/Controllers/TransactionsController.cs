using FinLedger.Contracts.Transactions;
using FinLedger.Transactions.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Transactions.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionUseCaseService _transactions;

    public TransactionsController(ITransactionUseCaseService transactions)
    {
        _transactions = transactions;
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create(CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactions.CreateTransactionAsync(
            new CreateTransactionCommand(
                request.PayerParticipantId,
                request.ReceiverParticipantId,
                request.PayerAccountId,
                request.ReceiverAccountId,
                request.PayerBankCode,
                request.PayerAccountNumber,
                request.ReceiverBankCode,
                request.ReceiverAccountNumber,
                request.Amount,
                request.CurrencyCode,
                request.Description,
                request.ExternalReference),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public Task<TransactionDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        return _transactions.GetTransactionByIdAsync(new GetTransactionByIdQuery(id), cancellationToken);
    }

    [HttpGet("by-number/{transactionNumber}")]
    public Task<TransactionDto> GetByNumber(string transactionNumber, CancellationToken cancellationToken)
    {
        return _transactions.GetTransactionByNumberAsync(new GetTransactionByNumberQuery(transactionNumber), cancellationToken);
    }

    [HttpGet]
    public Task<IReadOnlyCollection<TransactionDto>> GetTransactions(
        [FromQuery] string? status,
        [FromQuery] Guid? participantId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        return _transactions.GetTransactionsAsync(new GetTransactionsQuery(status, participantId, skip, take), cancellationToken);
    }

    [HttpPost("{id:guid}/process")]
    public Task<TransactionDto> Process(Guid id, CancellationToken cancellationToken)
    {
        return _transactions.ProcessTransactionAsync(new ProcessTransactionCommand(id), cancellationToken);
    }

    [HttpPost("{id:guid}/cancel")]
    public Task<TransactionDto> Cancel(Guid id, CancelTransactionRequest request, CancellationToken cancellationToken)
    {
        return _transactions.CancelTransactionAsync(new CancelTransactionCommand(id, request.Reason), cancellationToken);
    }
}
