using System;
using MediatR;

namespace EtrmService.Application.Pluvia.Commands;

public class RunHydrologicalSimulationCommand : IRequest<Guid>
{
    public Guid ScenarioId { get; set; }
    public string TargetSubmarket { get; set; } = string.Empty;
}
