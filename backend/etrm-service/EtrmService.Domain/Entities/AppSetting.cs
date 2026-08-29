using System;

namespace EtrmService.Domain.Entities;

public class AppSetting
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    protected AppSetting() { }

    public AppSetting(Guid tenantId, string key, string value)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Key = key;
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateValue(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}