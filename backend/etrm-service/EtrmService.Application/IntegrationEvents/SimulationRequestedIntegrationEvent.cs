using System;

namespace EtrmService.Application.IntegrationEvents;

public class SimulationRequestedIntegrationEvent
{
    public Guid SimulationId { get; set; }
    public Guid ScenarioId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string TargetSubmarket { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
