using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers;

public class EnaCalculatedEventConsumer : IConsumer<EnaCalculatedIntegrationEvent>
{
    private readonly IEtrmDbContext _context;
    private readonly ILogger<EnaCalculatedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public EnaCalculatedEventConsumer(IEtrmDbContext context, ILogger<EnaCalculatedEventConsumer> logger, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
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

        // Sprint 7: Disparo do Webhook
        try
        {
            var webhookPayload = new
            {
                Event = "ENA_CALCULATED",
                ExecutionId = message.ExecutionId,
                Submarket = message.Submarket,
                Timestamp = DateTime.UtcNow
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(webhookPayload), Encoding.UTF8, "application/json");
            
            // Exemplo fictício de URL B2B de um cliente
            var mockCustomerWebhookUrl = "http://b2b-customer.internal/api/webhooks/pluvia";
            
            // Dispara sem esperar (fire-and-forget style) ou await se for critico
            // Num cenário real usaria um Retry Policy (Polly)
            var response = await client.PostAsync(mockCustomerWebhookUrl, content, context.CancellationToken);
            
            _logger.LogInformation($"Webhook disparado para {mockCustomerWebhookUrl} com Status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Falha ao disparar webhook de ENA para a simulação {message.ExecutionId}. Erro: {ex.Message}");
        }
    }
}
