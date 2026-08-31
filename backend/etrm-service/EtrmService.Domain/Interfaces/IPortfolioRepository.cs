using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;

namespace EtrmService.Domain.Interfaces;

public interface IPortfolioRepository
{
    Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default);
    Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> GetDefaultPortfolioIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default);
}
