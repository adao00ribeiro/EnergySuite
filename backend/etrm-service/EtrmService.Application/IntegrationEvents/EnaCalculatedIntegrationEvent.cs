using System;

namespace EtrmService.Application.IntegrationEvents;

public class EnaCalculatedIntegrationEvent
{
    public Guid ExecutionId { get; set; }
    public string Submarket { get; set; } = string.Empty;
    public string Basin { get; set; } = string.Empty;
    public decimal ValueMwMed { get; set; }
    public decimal ValuePercentageMlt { get; set; }
    public DateTime TargetDate { get; set; }
}
