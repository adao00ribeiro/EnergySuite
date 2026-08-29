using System;
using System.Threading.Tasks;
using EtrmService.Application.Prospect.Services;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Operations.Services;

public interface IWebhookNotifierService
{
    Task NotifyRiskViolationAsync(Guid opportunityId, string reason);
    Task NotifyApprovedAsync(Guid operationId, string message);
}

public class WebhookNotifierService : IWebhookNotifierService
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<WebhookNotifierService> _logger;

    public WebhookNotifierService(IWebhookService webhookService, ILogger<WebhookNotifierService> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    public async Task NotifyRiskViolationAsync(Guid opportunityId, string reason)
    {
        var payload = new
        {
            Event = "OPERATION_RISK_VIOLATION",
            OpportunityId = opportunityId,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };

        await _webhookService.SendWebhookAsync("operation.risk-violation", payload);
        _logger.LogDebug("B2B credit-risk alert for Opportunity {OpportunityId} sent to webhook dispatch.", opportunityId);
    }

    public async Task NotifyApprovedAsync(Guid operationId, string message)
    {
        var payload = new
        {
            Event = "OPERATION_APPROVED",
            OperationId = operationId,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        await _webhookService.SendWebhookAsync("operation.approved", payload);
        _logger.LogDebug("B2B approval alert for Operation {OperationId} sent to webhook dispatch.", operationId);
    }
}