using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using EtrmService.Infrastructure.Data;

namespace EtrmService.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly EtrmDbContext _dbContext;

    public AuditLogRepository(EtrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
