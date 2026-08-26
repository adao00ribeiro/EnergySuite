using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class Portfolio
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Type { get; private set; } = string.Empty; // e.g., Trading, Varejo, Atacado
    public string Responsible { get; private set; } = string.Empty;
    
    public PortfolioStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Portfolio() { }

    public Portfolio(string name, string type, string responsible, string? description = null, Guid tenantId = default)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId == default ? Guid.Parse("00000000-0000-0000-0000-000000000001") : tenantId;
        Name = name;
        Type = type;
        Responsible = responsible;
        Description = description;
        Status = PortfolioStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(PortfolioStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
