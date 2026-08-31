using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Application.Services;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers;

public class EnaCalculatedEventConsumer : IConsumer<EnaCalculatedIntegrationEvent>
{
    private readonly IHydrologyRepository _hydrologyRepository;
    private readonly IWebhookRepository _webhookRepository;
    private readonly ILogger<EnaCalculatedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public EnaCalculatedEventConsumer(
        IHydrologyRepository hydrologyRepository,
        IWebhookRepository webhookRepository,
        ILogger<EnaCalculatedEventConsumer> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _hydrologyRepository = hydrologyRepository;
        _webhookRepository = webhookRepository;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<EnaCalculatedIntegrationEvent> context)
    {
        var message = context.Message;

        var result = new HydrologicalResult(
            executionId: message.ExecutionId,
            submarket: message.Submarket,
            basin: message.Basin,
            valueMwMed: message.ValueMwMed,
            valuePercentageMlt: message.ValuePercentageMlt,
            targetDate: message.TargetDate
        );

        await _hydrologyRepository.AddResultAsync(result, context.CancellationToken);

        _logger.LogInformation($"Saved ENA result for Execution {message.ExecutionId} - {message.Submarket}: {message.ValueMwMed} MWmed ({message.ValuePercentageMlt}%)");

        await DispatchWebhookAsync(message, context.CancellationToken);
    }

    private async Task DispatchWebhookAsync(EnaCalculatedIntegrationEvent message, System.Threading.CancellationToken cancellationToken)
    {
        var webhookPayload = new
        {
            Event = "ENA_CALCULATED",
            ExecutionId = message.ExecutionId,
            Submarket = message.Submarket,
            Basin = message.Basin,
            ValueMwMed = message.ValueMwMed,
            ValuePercentageMlt = message.ValuePercentageMlt,
            TargetDate = message.TargetDate,
            Timestamp = DateTime.UtcNow
        };

        var jsonPayload = JsonSerializer.Serialize(webhookPayload);

        var subscriptions = (await _webhookRepository.GetActiveSubscriptionsAsync(cancellationToken)).ToList();

        var configuredCustomerUrl = _configuration["Webhooks:Customer:BaseUrl"];
        var configuredSecret = _configuration["Webhooks:DefaultSecretKey"] ?? string.Empty;

        var dispatchTargets = new List<(string Url, string Secret)>();
        if (subscriptions.Count > 0)
        {
            dispatchTargets.AddRange(subscriptions.Select(s => (s.Url, s.SecretKey)));
        }
        else if (!string.IsNullOrWhiteSpace(configuredCustomerUrl))
        {
            dispatchTargets.Add((configuredCustomerUrl, configuredSecret));
        }

        if (dispatchTargets.Count == 0)
        {
            _logger.LogWarning(
                "ENA webhook dispatch skipped for Execution {ExecutionId}: no active WebhookSubscription and 'Webhooks:Customer:BaseUrl' is not configured.",
                message.ExecutionId);
            return;
        }

        var client = _httpClientFactory.CreateClient("WebhookClient");

        foreach (var (url, secret) in dispatchTargets)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };

                var signature = WebhookSigningService.ComputeSignature(secret, jsonPayload);
                if (!string.IsNullOrEmpty(signature))
                    request.Headers.Add("X-EnergySuite-Signature", signature);

                var response = await client.SendAsync(request, cancellationToken);

                _logger.LogInformation($"ENA webhook dispatched to {url} for Execution {message.ExecutionId} with Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to dispatch ENA webhook to {url} for Execution {message.ExecutionId}");
            }
        }
    }
}