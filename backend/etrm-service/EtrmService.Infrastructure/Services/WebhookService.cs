using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EtrmService.Application.Prospect.Services;
using Microsoft.Extensions.Logging;

namespace EtrmService.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(ILogger<WebhookService> logger)
    {
        _logger = logger;
    }

    public Task SendWebhookAsync(string eventName, object payload)
    {
        // Mock implementation of a Webhook trigger.
        var json = JsonSerializer.Serialize(payload);
        
        _logger.LogInformation("==================================================");
        _logger.LogInformation($"[WEBHOOK FIRED] Event: {eventName}");
        _logger.LogInformation($"[WEBHOOK PAYLOAD]: {json}");
        _logger.LogInformation("==================================================");

        return Task.CompletedTask;
    }
}
