using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EtrmService.Application.Prospect.Services;
using Microsoft.Extensions.Logging;

namespace EtrmService.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public WebhookService(ILogger<WebhookService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendWebhookAsync(string eventName, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        
        _logger.LogInformation("==================================================");
        _logger.LogInformation($"[WEBHOOK FIRED] Event: {eventName}");
        _logger.LogInformation($"[WEBHOOK PAYLOAD]: {json}");
        _logger.LogInformation("==================================================");

        try
        {
            var client = _httpClientFactory.CreateClient("WebhookClient");
            // Fallback to a default if not configured, or use configuration in a real scenario
            client.BaseAddress ??= new System.Uri("https://webhook.site/energy-suite-events"); 

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/events/{eventName}", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Webhook for event {eventName} successfully delivered.");
            }
            else
            {
                _logger.LogWarning($"Webhook delivery failed. Status Code: {response.StatusCode}");
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, $"Error delivering webhook for event {eventName}");
        }
    }
}
