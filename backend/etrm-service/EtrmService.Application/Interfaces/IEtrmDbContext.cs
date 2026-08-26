using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Interfaces;

public interface IEtrmDbContext
{
    DbSet<Contract> Contracts { get; }
    DbSet<PrecipitationScenario> PrecipitationScenarios { get; }
    DbSet<ModelExecution> ModelExecutions { get; }
    DbSet<HydrologicalResult> HydrologicalResults { get; }
    DbSet<ForecastMetadata> ForecastMetadatas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
