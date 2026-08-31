using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.AuditLogs.Queries;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string ChangesJson { get; set; } = null!;
    public string ChangedBy { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
}

public class GetAuditLogsByUserQuery : IRequest<List<AuditLogDto>>
{
    public string UserId { get; set; } = null!;
}

public class GetAuditLogsByUserQueryHandler : IRequestHandler<GetAuditLogsByUserQuery, List<AuditLogDto>>
{
    private readonly IEtrmDbContext _context;

    public GetAuditLogsByUserQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuditLogDto>> Handle(GetAuditLogsByUserQuery request, CancellationToken cancellationToken)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.ChangedBy == request.UserId)
            .OrderByDescending(l => l.ChangedAt)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Action = l.Action,
                ChangesJson = l.ChangesJson,
                ChangedBy = l.ChangedBy,
                ChangedAt = l.ChangedAt
            })
            .ToListAsync(cancellationToken);
    }
}
