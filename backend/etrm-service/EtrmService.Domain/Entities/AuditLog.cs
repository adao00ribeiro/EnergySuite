using System;

namespace EtrmService.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public string EntityName { get; private set; }
    public string EntityId { get; private set; }
    public string Action { get; private set; } // Created, Updated, Deleted, StateChanged
    public string ChangesJson { get; private set; }
    public string ChangedBy { get; private set; } // Username or Id
    public DateTime ChangedAt { get; private set; }
    public Guid TenantId { get; private set; }

    protected AuditLog() { }

    public AuditLog(string entityName, string entityId, string action, string changesJson, string changedBy, Guid tenantId)
    {
        Id = Guid.NewGuid();
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        ChangesJson = changesJson;
        ChangedBy = changedBy;
        ChangedAt = DateTime.UtcNow;
        TenantId = tenantId;
    }
}
