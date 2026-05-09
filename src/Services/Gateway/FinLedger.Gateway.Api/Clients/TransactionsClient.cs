using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinLedger.Gateway.Api.Contracts.Requests;
using FinLedger.Gateway.Api.Contracts.Responses;
using FinLedger.Gateway.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Gateway.Api.Clients;

public sealed class TransactionsClient : ITransactionsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TransactionsClient> _logger;

    public TransactionsClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TransactionsClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<CreateTransactionClientResponse> CreateTransactionAsync(
        CreateTransactionClientRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, "api/transactions");
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);
        return await SendAsync<CreateTransactionClientResponse>(httpRequest, cancellationToken);
    }

    public async Task<ProcessTransactionClientResponse> ProcessTransactionAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, $"api/transactions/{transactionId}/process");
        return await SendAsync<ProcessTransactionClientResponse>(httpRequest, cancellationToken);
    }

    public async Task<TransactionDetailsClientResponse> GetTransactionByIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Get, $"api/transactions/{transactionId}");
        return await SendAsync<TransactionDetailsClientResponse>(httpRequest, cancellationToken);
    }

    public async Task<TransactionDetailsClientResponse> GetTransactionByNumberAsync(
        string transactionNumber,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Get, $"api/transactions/by-number/{Uri.EscapeDataString(transactionNumber)}");
        return await SendAsync<TransactionDetailsClientResponse>(httpRequest, cancellationToken);
    }

    public async Task<TransactionDetailsClientResponse> CancelTransactionAsync(
        Guid transactionId,
        CancelTransactionClientRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, $"api/transactions/{transactionId}/cancel");
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);
        return await SendAsync<TransactionDetailsClientResponse>(httpRequest, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, GetCorrelationId());
        return request;
    }

    private string GetCorrelationId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Items.TryGetValue(CorrelationIdMiddleware.ItemName, out var correlationId) == true &&
            correlationId is not null)
        {
            return correlationId.ToString()!;
        }

        return Guid.NewGuid().ToString("N");
    }

    private async Task<TResponse> SendAsync<TResponse>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new GatewayTimeoutException("Transaction Service did not respond in time.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new GatewayBadGatewayException("Transaction Service is unavailable.", exception);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await ThrowGatewayExceptionAsync(response, cancellationToken);
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
            if (result is null)
            {
                throw new GatewayBadGatewayException("Transaction Service returned an empty response.");
            }

            return result;
        }
    }

    private async Task ThrowGatewayExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var detail = await ReadProblemDetailAsync(response, cancellationToken)
            ?? $"Transaction Service returned {(int)response.StatusCode} {response.ReasonPhrase}.";

        _logger.LogWarning("Transaction Service returned HTTP {StatusCode}: {Detail}", (int)response.StatusCode, detail);

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new GatewayValidationException(detail),
            HttpStatusCode.NotFound => new GatewayNotFoundException(detail),
            HttpStatusCode.Conflict => new GatewayConflictException(detail),
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => new GatewayTimeoutException(detail),
            _ => new GatewayBadGatewayException(detail)
        };
    }

    private static async Task<string?> ReadProblemDetailAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions, cancellationToken);
            return problem?.Detail ?? problem?.Title;
        }
        catch (JsonException)
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
