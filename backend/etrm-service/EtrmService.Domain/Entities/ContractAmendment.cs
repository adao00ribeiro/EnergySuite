using System;

namespace EtrmService.Domain.Entities;

public class ContractAmendment
{
    public Guid Id { get; private set; }
    public Guid ContractId { get; private set; }
    public int Version { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime EffectiveDate { get; private set; }
    public decimal PreviousPrice { get; private set; }
    public decimal NewPrice { get; private set; }
    public decimal PreviousVolumeMwMed { get; private set; }
    public decimal NewVolumeMwMed { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation
    public virtual Contract Contract { get; private set; } = null!;

    protected ContractAmendment() { } // EF Core

    public ContractAmendment(Guid contractId, int version, string description, DateTime effectiveDate, decimal previousPrice, decimal newPrice, decimal previousVolumeMwMed, decimal newVolumeMwMed)
    {
        Id = Guid.NewGuid();
        ContractId = contractId;
        Version = version;
        Description = description;
        EffectiveDate = DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc);
        PreviousPrice = previousPrice;
        NewPrice = newPrice;
        PreviousVolumeMwMed = previousVolumeMwMed;
        NewVolumeMwMed = newVolumeMwMed;
        CreatedAt = DateTime.UtcNow;
    }
}
