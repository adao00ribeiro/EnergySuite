using System;

namespace EtrmService.Application.IntegrationEvents
{
    public class HydrologicalSimulationProgressEvent
    {
        public Guid SimulationId { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public int Percentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
