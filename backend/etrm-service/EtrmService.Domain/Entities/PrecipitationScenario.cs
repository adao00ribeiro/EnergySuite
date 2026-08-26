using System;
using System.Collections.Generic;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class PrecipitationScenario
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ScenarioSource SourceType { get; private set; }
    public DateTime ReferenceDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int HorizonDays { get; private set; }
    
    // Navigation property
    public ICollection<ModelExecution> Executions { get; private set; } = new List<ModelExecution>();

    // For EF Core
    protected PrecipitationScenario() { }

    public PrecipitationScenario(string name, ScenarioSource sourceType, DateTime referenceDate, int horizonDays)
    {
        Id = Guid.NewGuid();
        Name = name;
        SourceType = sourceType;
        ReferenceDate = referenceDate;
        HorizonDays = horizonDays;
        CreatedAt = DateTime.UtcNow;
    }
}
