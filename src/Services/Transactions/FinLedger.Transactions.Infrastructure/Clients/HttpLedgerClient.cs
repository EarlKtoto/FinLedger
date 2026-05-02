using System.Net.Http.Json;
using FinLedger.Contracts.Ledger;
using FinLedger.Transactions.Application.Abstractions;
using FinLedger.Transactions.Domain.Entities;

namespace FinLedger.Transactions.Infrastructure.Clients;

public sealed class HttpLedgerClient : ILedgerClient
{
    private readonly HttpClient _httpClient;

    public HttpLedgerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LedgerReservationResult> ReserveFundsAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var request = new ReserveFundsRequest(
            transaction.PayerAccountId,
            transaction.ExternalReference ?? transaction.TransactionNumber,
            $"{transaction.TransactionNumber}:reserve",
            transaction.Amount,
            transaction.CurrencyCode,
            null,
            transaction.Description);

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ledger/reservations", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return LedgerReservationResult.Failure($"Ledger reservation failed with HTTP {(int)response.StatusCode}.");
            }

            var reservation = await response.Content.ReadFromJsonAsync<FundsReservationDto>(cancellationToken);
            return reservation is null
                ? LedgerReservationResult.Failure("Ledger reservation returned an empty response.")
                : LedgerReservationResult.Success(reservation.Id, "Funds reserved.");
        }
        catch (HttpRequestException exception)
        {
            return LedgerReservationResult.Failure($"Ledger reservation failed: {exception.Message}");
        }
    }

    public async Task<LedgerCommitResult> CommitTransferAsync(Transaction transaction, Guid reservationId, CancellationToken cancellationToken = default)
    {
        var request = new CaptureReservationRequest(
            transaction.ReceiverAccountId,
            $"{transaction.TransactionNumber}:capture",
            transaction.ExternalReference ?? transaction.TransactionNumber,
            transaction.Description);

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/ledger/reservations/{reservationId}/capture", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return LedgerCommitResult.Failure($"Ledger capture failed with HTTP {(int)response.StatusCode}.");
            }

            var ledgerTransaction = await response.Content.ReadFromJsonAsync<LedgerTransactionDto>(cancellationToken);
            return ledgerTransaction is null
                ? LedgerCommitResult.Failure("Ledger capture returned an empty response.")
                : LedgerCommitResult.Success(ledgerTransaction.Id, "Ledger transfer committed.");
        }
        catch (HttpRequestException exception)
        {
            return LedgerCommitResult.Failure($"Ledger capture failed: {exception.Message}");
        }
    }

    public async Task<ExternalOperationResult> ReverseAsync(Transaction transaction, Guid? reservationId, Guid? ledgerTransactionId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            if (ledgerTransactionId.HasValue)
            {
                var reverseRequest = new ReverseLedgerTransactionRequest(
                    $"{transaction.TransactionNumber}:reverse",
                    $"reverse:{transaction.ExternalReference ?? transaction.TransactionNumber}",
                    reason);
                var reverseResponse = await _httpClient.PostAsJsonAsync($"/api/ledger/transactions/{ledgerTransactionId.Value}/reverse", reverseRequest, cancellationToken);
                if (reverseResponse.IsSuccessStatusCode)
                {
                    return ExternalOperationResult.Success("Ledger transaction reversed.");
                }

                return ExternalOperationResult.Failure($"Ledger reverse failed with HTTP {(int)reverseResponse.StatusCode}.");
            }

            if (reservationId.HasValue)
            {
                var releaseRequest = new ReleaseReservationRequest($"{transaction.TransactionNumber}:release", reason);
                var releaseResponse = await _httpClient.PostAsJsonAsync($"/api/ledger/reservations/{reservationId.Value}/release", releaseRequest, cancellationToken);
                if (releaseResponse.IsSuccessStatusCode)
                {
                    return ExternalOperationResult.Success("Ledger reservation released.");
                }

                return ExternalOperationResult.Failure($"Ledger reservation release failed with HTTP {(int)releaseResponse.StatusCode}.");
            }

            return ExternalOperationResult.Success("No ledger operation needed.");
        }
        catch (HttpRequestException exception)
        {
            return ExternalOperationResult.Failure($"Ledger reverse failed: {exception.Message}");
        }
    }
}
