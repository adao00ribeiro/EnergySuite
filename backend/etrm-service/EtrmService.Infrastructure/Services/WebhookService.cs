using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EtrmService.Application.Prospect.Services;
using EtrmService.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EtrmService.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _defaultSecretKey;

    public WebhookService(ILogger<WebhookService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _defaultSecretKey = configuration["Webhooks:DefaultSecretKey"];
    }

    public async Task SendWebhookAsync(string eventName, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var client = _httpClientFactory.CreateClient("WebhookClient");

        // BK-12(b): sem default público. Se não configurado, o disparo não ocorre.
        if (client.BaseAddress == null)
        {
            _logger.LogWarning(
                "Webhook for event {EventName} skipped: 'Webhooks:DefaultBaseAddress' is not configured.",
                eventName);
            return;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/events/{eventName}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Assinatura HMAC-SHA256 do payload canonizado (BK-12).
            var signature = WebhookSigningService.ComputeSignature(_defaultSecretKey ?? string.Empty, json);
            if (!string.IsNullOrEmpty(signature))
                request.Headers.Add("X-EnergySuite-Signature", signature);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Webhook for event {eventName} successfully delivered.");
            }
            else
            {
                _logger.LogWarning($"Webhook delivery failed. Status Code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error delivering webhook for event {eventName}");
        }
    }
}