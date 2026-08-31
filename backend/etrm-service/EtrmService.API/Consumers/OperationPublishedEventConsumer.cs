using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Application.Services;
using EtrmService.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers;

public class OperationPublishedEventConsumer : IConsumer<OperationPublishedIntegrationEvent>
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly ILogger<OperationPublishedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public OperationPublishedEventConsumer(IWebhookRepository webhookRepository, ILogger<OperationPublishedEventConsumer> logger, IHttpClientFactory httpClientFactory)
    {
        _webhookRepository = webhookRepository;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Consume(ConsumeContext<OperationPublishedIntegrationEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation($"Operation {message.OperationId} published. Checking for webhooks...");

        var subscriptions = (await _webhookRepository.GetActiveSubscriptionsByCompanyAsync(message.CounterpartyId, context.CancellationToken)).ToList();

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

