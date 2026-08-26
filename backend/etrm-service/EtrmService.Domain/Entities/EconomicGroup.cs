using System;
using System.Collections.Generic;

namespace EtrmService.Domain.Entities;

public class EconomicGroup
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    
    private readonly List<Company> _companies = new();
    public IReadOnlyCollection<Company> Companies => _companies.AsReadOnly();

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected EconomicGroup() { }

    public EconomicGroup(string name, Guid tenantId = default)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId == default ? Guid.Parse("00000000-0000-0000-0000-000000000001") : tenantId;
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }
}
