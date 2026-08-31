using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;

namespace EtrmService.Domain.Interfaces;

public interface IHydrologyRepository
{
    Task AddResultAsync(HydrologicalResult result, CancellationToken cancellationToken = default);
    Task<PrecipitationScenario?> GetLatestScenarioAsync(CancellationToken cancellationToken = default);
}
