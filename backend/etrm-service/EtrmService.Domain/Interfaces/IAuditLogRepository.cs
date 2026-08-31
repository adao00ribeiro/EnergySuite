using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;

namespace EtrmService.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
