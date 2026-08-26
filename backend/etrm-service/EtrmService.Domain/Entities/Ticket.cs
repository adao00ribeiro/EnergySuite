using System;
using System.Collections.Generic;

namespace EtrmService.Domain.Entities;

public class Ticket
{
    public Guid Id { get; private set; }
    public string ReferenceNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid TenantId { get; private set; }
    
    // Navigation properties
    public ICollection<Operation> Operations { get; private set; }

    protected Ticket() 
    {
        Operations = new List<Operation>();
    }

    public Ticket(string referenceNumber, Guid tenantId) : this()
    {
        Id = Guid.NewGuid();
        ReferenceNumber = referenceNumber;
        CreatedAt = DateTime.UtcNow;
        TenantId = tenantId;
    }
}
