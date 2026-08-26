using System;

namespace EtrmService.Domain.Entities;

public class HydrologicalResult
{
    public Guid Id { get; private set; }
    public Guid ExecutionId { get; private set; }
    public string Submarket { get; private set; } = string.Empty;
    public string Basin { get; private set; } = string.Empty;
    public decimal ValueMwMed { get; private set; }
    public decimal ValuePercentageMlt { get; private set; }
    public DateTime TargetDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public ModelExecution Execution { get; private set; } = null!;

    // For EF Core
    protected HydrologicalResult() { }

    public HydrologicalResult(Guid executionId, string submarket, string basin, decimal valueMwMed, decimal valuePercentageMlt, DateTime targetDate)
    {
        Id = Guid.NewGuid();
        ExecutionId = executionId;
        Submarket = submarket;
        Basin = basin;
        ValueMwMed = valueMwMed;
        ValuePercentageMlt = valuePercentageMlt;
        TargetDate = targetDate;
        CreatedAt = DateTime.UtcNow;
    }
}
