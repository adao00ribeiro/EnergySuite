using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Operations.Services;

public interface IWebhookNotifierService
{
    Task NotifyRiskViolationAsync(Guid opportunityId, string reason);
}

public class WebhookNotifierService : IWebhookNotifierService
{
    private readonly ILogger<WebhookNotifierService> _logger;

    public WebhookNotifierService(ILogger<WebhookNotifierService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyRiskViolationAsync(Guid opportunityId, string reason)
    {
        await Task.Delay(50); // Simulate HTTP POST to webhook.site or internal B2B broker
        
        _logger.LogWarning("[WEBHOOK DISPATCHED] B2B Alert: Credit Risk Violation on Opportunity {OpportunityId}. Reason: {Reason}", opportunityId, reason);
    }
}
