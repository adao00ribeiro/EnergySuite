using System;

namespace EtrmService.Application.Prospect.Events;

public class StudyExecutionRequestedEvent
{
    public Guid StudyId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime RequestedAt { get; set; }
}
