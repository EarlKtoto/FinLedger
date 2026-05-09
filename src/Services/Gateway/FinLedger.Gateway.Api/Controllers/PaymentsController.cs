using FinLedger.Gateway.Api.Clients;
using FinLedger.Gateway.Api.Contracts.Requests;
using FinLedger.Gateway.Api.Contracts.Responses;
using FinLedger.Gateway.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Gateway.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly ITransactionsClient _transactionsClient;

    public PaymentsController(ITransactionsClient transactionsClient)
    {
        _transactionsClient = transactionsClient;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateCreatePayment(request);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var transactionRequest = new CreateTransactionClientRequest(
            request.PayerParticipantId,
            request.ReceiverParticipantId,
            request.PayerAccountId,
            request.ReceiverAccountId,
            request.PayerBankCode.Trim(),
            request.PayerAccountNumber.Trim(),
            request.ReceiverBankCode.Trim(),
            request.ReceiverAccountNumber.Trim(),
            request.Amount,
            request.Currency.Trim().ToUpperInvariant(),
            request.Description,
            request.ExternalReference);

        var created = await _transactionsClient.CreateTransactionAsync(transactionRequest, cancellationToken);
        var processed = await _transactionsClient.ProcessTransactionAsync(created.Id, cancellationToken);
        return Ok(Map(processed));
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<PaymentResponse>> GetById(Guid transactionId, CancellationToken cancellationToken)
    {
        var transaction = await _transactionsClient.GetTransactionByIdAsync(transactionId, cancellationToken);
        return Ok(Map(transaction));
    }

    [HttpGet("by-number/{transactionNumber}")]
    public async Task<ActionResult<PaymentResponse>> GetByNumber(
        string transactionNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(transactionNumber))
        {
            throw new GatewayValidationException("Transaction number is required.");
        }

        var transaction = await _transactionsClient.GetTransactionByNumberAsync(transactionNumber.Trim(), cancellationToken);
        return Ok(Map(transaction));
    }

    [HttpPost("{transactionId:guid}/cancel")]
    public async Task<ActionResult<PaymentResponse>> Cancel(
        Guid transactionId,
        CancelPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionsClient.CancelTransactionAsync(
            transactionId,
            new CancelTransactionClientRequest(request.Reason),
            cancellationToken);

        return Ok(Map(transaction));
    }

    private ActionResult? ValidateCreatePayment(CreatePaymentRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        AddGuidRequired(errors, nameof(request.PayerParticipantId), request.PayerParticipantId);
        AddGuidRequired(errors, nameof(request.PayerAccountId), request.PayerAccountId);
        AddGuidRequired(errors, nameof(request.ReceiverParticipantId), request.ReceiverParticipantId);
        AddGuidRequired(errors, nameof(request.ReceiverAccountId), request.ReceiverAccountId);
        AddRequired(errors, nameof(request.PayerBankCode), request.PayerBankCode);
        AddRequired(errors, nameof(request.PayerAccountNumber), request.PayerAccountNumber);
        AddRequired(errors, nameof(request.ReceiverBankCode), request.ReceiverBankCode);
        AddRequired(errors, nameof(request.ReceiverAccountNumber), request.ReceiverAccountNumber);

        if (request.PayerAccountId != Guid.Empty &&
            request.ReceiverAccountId != Guid.Empty &&
            request.PayerAccountId == request.ReceiverAccountId)
        {
            errors[nameof(request.ReceiverAccountId)] = ["PayerAccountId and ReceiverAccountId must be different."];
        }

        if (request.Amount <= 0)
        {
            errors[nameof(request.Amount)] = ["Amount must be greater than zero."];
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors[nameof(request.Currency)] = ["Currency is required."];
        }
        else if (request.Currency.Trim().Length != 3)
        {
            errors[nameof(request.Currency)] = ["Currency must contain exactly three characters."];
        }

        if (request.Description?.Length > 500)
        {
            errors[nameof(request.Description)] = ["Description must not exceed 500 characters."];
        }

        if (request.ExternalReference?.Length > 100)
        {
            errors[nameof(request.ExternalReference)] = ["ExternalReference must not exceed 100 characters."];
        }

        if (errors.Count == 0)
        {
            return null;
        }

        var problem = new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Instance = HttpContext.Request.Path
        };

        problem.Extensions["correlationId"] = GetCorrelationId();
        return BadRequest(problem);
    }

    private static void AddGuidRequired(IDictionary<string, string[]> errors, string name, Guid value)
    {
        if (value == Guid.Empty)
        {
            errors[name] = [$"{name} is required."];
        }
    }

    private static void AddRequired(IDictionary<string, string[]> errors, string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[name] = [$"{name} is required."];
        }
    }

    private PaymentResponse Map(ProcessTransactionClientResponse transaction)
    {
        return new PaymentResponse(
            transaction.Id,
            transaction.TransactionNumber,
            transaction.Status,
            transaction.PayerParticipantId,
            transaction.PayerAccountId,
            transaction.ReceiverParticipantId,
            transaction.ReceiverAccountId,
            transaction.Amount,
            transaction.CurrencyCode,
            transaction.Description,
            transaction.ExternalReference,
            transaction.FailureReason,
            transaction.LedgerReservationId,
            transaction.LedgerTransactionId,
            GetCorrelationId(),
            transaction.CreatedAtUtc,
            transaction.CompletedAtUtc,
            transaction.CancelledAtUtc);
    }

    private PaymentResponse Map(TransactionDetailsClientResponse transaction)
    {
        return new PaymentResponse(
            transaction.Id,
            transaction.TransactionNumber,
            transaction.Status,
            transaction.PayerParticipantId,
            transaction.PayerAccountId,
            transaction.ReceiverParticipantId,
            transaction.ReceiverAccountId,
            transaction.Amount,
            transaction.CurrencyCode,
            transaction.Description,
            transaction.ExternalReference,
            transaction.FailureReason,
            transaction.LedgerReservationId,
            transaction.LedgerTransactionId,
            GetCorrelationId(),
            transaction.CreatedAtUtc,
            transaction.CompletedAtUtc,
            transaction.CancelledAtUtc);
    }

    private string GetCorrelationId()
    {
        return HttpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemName, out var correlationId) &&
               correlationId is not null
            ? correlationId.ToString()!
            : Guid.NewGuid().ToString("N");
    }
}
