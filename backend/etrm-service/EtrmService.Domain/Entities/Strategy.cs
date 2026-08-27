using System;

namespace EtrmService.Domain.Entities;

public class Strategy
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    protected Strategy() { }

    public Strategy(string name, string description, Guid tenantId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
