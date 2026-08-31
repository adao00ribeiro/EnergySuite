using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;

namespace EtrmService.Domain.Interfaces;

public interface IWebhookRepository
{
    Task AddSubscriptionAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookSubscription>> GetActiveSubscriptionsByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
}
