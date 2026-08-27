using System;

namespace EtrmService.Domain.Entities;

public class Simulation
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OpportunityId { get; private set; }
    public Guid UserId { get; private set; }

    public string Notes { get; private set; } = string.Empty;
    
    public decimal NetPositionBeforeMwMed { get; private set; }
    public decimal NetPositionAfterMwMed { get; private set; }
    
    public DateTime SimulatedAt { get; private set; }

    public Opportunity Opportunity { get; private set; } = null!;

    protected Simulation() { }

    public Simulation(Guid opportunityId, decimal netPositionBefore, decimal netPositionAfter, Guid userId, string notes, Guid tenantId)
    {
        Id = Guid.NewGuid();
        OpportunityId = opportunityId;
        NetPositionBeforeMwMed = netPositionBefore;
        NetPositionAfterMwMed = netPositionAfter;
        UserId = userId;
        Notes = notes;
        TenantId = tenantId;
        SimulatedAt = DateTime.UtcNow;
    }
}
