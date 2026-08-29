using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Services;
using EtrmService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers;

public class EnaCalculatedEventConsumer : IConsumer<EnaCalculatedIntegrationEvent>
{
    private readonly IEtrmDbContext _context;
    private readonly ILogger<EnaCalculatedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public EnaCalculatedEventConsumer(
        IEtrmDbContext context,
        ILogger<EnaCalculatedEventConsumer> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _context = context;
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

        _context.HydrologicalResults.Add(result);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation($"Saved ENA result for Execution {message.ExecutionId} - {message.Submarket}: {message.ValueMwMed} MWmed ({message.ValuePercentageMlt}%)");

        // Disparo do Webhook B2B (Sprint 7 / BK-12b): URL vem de subscription ou configuração.
        // Nenhuma URL pública de teste é usada como default.
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

        // Preferência 1: subscriptions ativas registradas no banco (URL + secret por assinante)
        var subscriptions = await _context.WebhookSubscriptions
            .Where(w => w.IsActive)
            .ToListAsync(cancellationToken);

        // Preferência 2: URL configurada (Webhooks:Customer:BaseUrl)
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