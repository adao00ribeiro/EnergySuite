using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers;

public class OperationPublishedEventConsumer : IConsumer<OperationPublishedIntegrationEvent>
{
    private readonly IEtrmDbContext _context;
    private readonly ILogger<OperationPublishedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public OperationPublishedEventConsumer(IEtrmDbContext context, ILogger<OperationPublishedEventConsumer> logger, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Consume(ConsumeContext<OperationPublishedIntegrationEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation($"Operation {message.OperationId} published. Checking for webhooks...");

        var subscriptions = await _context.WebhookSubscriptions
            .Where(w => w.CompanyId == message.CounterpartyId && w.IsActive)
            .ToListAsync(context.CancellationToken);

        if (!subscriptions.Any())
        {
            _logger.LogInformation($"No active webhooks found for Counterparty {message.CounterpartyId}");
            return;
        }

        var payload = new
        {
            Event = "OPERATION_PUBLISHED",
            OperationId = message.OperationId,
            TicketId = message.TicketId,
            Status = message.Status,
            Volume = message.Volume,
            Price = message.Price,
            Timestamp = message.Timestamp
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        using var httpClient = _httpClientFactory.CreateClient("WebhookClient");

        foreach (var sub in subscriptions)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, sub.Url)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };

                // Assinatura HMAC-SHA256 do payload canonizado (JSON serializado).
                // A chave secreta do webhook nunca transita em texto claro no header.
                var signature = WebhookSigningService.ComputeSignature(sub.SecretKey, jsonPayload);
                if (!string.IsNullOrEmpty(signature))
                    request.Headers.Add("X-EnergySuite-Signature", signature);

                var response = await httpClient.SendAsync(request, context.CancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully sent webhook to {sub.Url} for Operation {message.OperationId}");
                }
                else
                {
                    _logger.LogWarning($"Webhook to {sub.Url} returned status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send webhook to {sub.Url}");
            }
        }
    }
}
