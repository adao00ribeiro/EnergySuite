using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using EtrmService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Infrastructure.Repositories;

public class HydrologyRepository : IHydrologyRepository
{
    private readonly EtrmDbContext _dbContext;

    public HydrologyRepository(EtrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddResultAsync(HydrologicalResult result, CancellationToken cancellationToken = default)
    {
        await _dbContext.HydrologicalResults.AddAsync(result, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PrecipitationScenario?> GetLatestScenarioAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PrecipitationScenarios
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
