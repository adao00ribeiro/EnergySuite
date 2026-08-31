using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using EtrmService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Infrastructure.Repositories;

public class WebhookRepository : IWebhookRepository
{
    private readonly EtrmDbContext _dbContext;

    public WebhookRepository(EtrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddSubscriptionAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _dbContext.WebhookSubscriptions.AddAsync(subscription, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<WebhookSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(w => w.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WebhookSubscription>> GetActiveSubscriptionsByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(w => w.CompanyId == companyId && w.IsActive)
            .ToListAsync(cancellationToken);
    }
}
