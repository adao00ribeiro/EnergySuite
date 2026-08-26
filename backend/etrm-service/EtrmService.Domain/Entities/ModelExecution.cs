using System;
using System.Collections.Generic;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class ModelExecution
{
    public Guid Id { get; private set; }
    public Guid ScenarioId { get; private set; }
    public HydrologicalModelType ModelType { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties
    public PrecipitationScenario Scenario { get; private set; } = null!;
    public ICollection<HydrologicalResult> Results { get; private set; } = new List<HydrologicalResult>();

    // For EF Core
    protected ModelExecution() { }

    public ModelExecution(Guid scenarioId, HydrologicalModelType modelType)
    {
        Id = Guid.NewGuid();
        ScenarioId = scenarioId;
        ModelType = modelType;
        Status = ExecutionStatus.Pending;
        StartedAt = DateTime.UtcNow;
    }

    public void MarkAsRunning()
    {
        Status = ExecutionStatus.Running;
    }

    public void MarkAsCompleted()
    {
        Status = ExecutionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = ExecutionStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }
}
