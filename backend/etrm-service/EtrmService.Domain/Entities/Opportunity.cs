using System;

namespace EtrmService.Domain.Entities;

public class Opportunity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Guid? StrategyId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public decimal EstimatedVolumeMwMed { get; private set; }
    public decimal EstimatedSpread { get; private set; }
    public decimal OpportunityScore { get; private set; }
    
    public string Status { get; private set; } = "Draft"; // Draft, Approved, Rejected, Executed

    public DateTime CreatedAt { get; private set; }
    
    public Portfolio Portfolio { get; private set; } = null!;
    public Strategy? Strategy { get; private set; }

    protected Opportunity() { }

    public Opportunity(string title, Guid portfolioId, decimal volumeMwMed, decimal spread, decimal score, Guid tenantId, Guid? strategyId = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        PortfolioId = portfolioId;
        StrategyId = strategyId;
        EstimatedVolumeMwMed = volumeMwMed;
        EstimatedSpread = spread;
        OpportunityScore = score;
        TenantId = tenantId;
        Status = "Draft";
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(string newStatus)
    {
        Status = newStatus;
    }
}
