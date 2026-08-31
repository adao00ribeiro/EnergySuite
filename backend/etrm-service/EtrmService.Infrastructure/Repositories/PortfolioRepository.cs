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

public class PortfolioRepository : IPortfolioRepository
{
    private readonly EtrmDbContext _dbContext;

    public PortfolioRepository(EtrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        await _dbContext.Portfolios.AddAsync(portfolio, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Guid> GetDefaultPortfolioIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Portfolios
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Portfolios
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
