using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Prospect.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Prospect.Commands;

public class ExecuteStudyCommandHandler : IRequestHandler<ExecuteStudyCommand, bool>
{
    private readonly IEtrmDbContext _context;
    private readonly IEventPublisher _eventPublisher;

    public ExecuteStudyCommandHandler(IEtrmDbContext context, IEventPublisher eventPublisher)
    {
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(ExecuteStudyCommand request, CancellationToken cancellationToken)
    {
        var study = await _context.ProspectStudies
            .FirstOrDefaultAsync(s => s.Id == request.StudyId && s.TenantId == request.TenantId, cancellationToken);

        if (study == null)
            throw new Exception("Study not found");

        // Update Study state
        study.ChangeState(Domain.Enums.StudyState.Queued);

        await _context.SaveChangesAsync(cancellationToken);

        // Publish Event to Message Broker
        var ev = new StudyExecutionRequestedEvent
        {
            StudyId = study.Id,
            TenantId = study.TenantId,
            RequestedAt = DateTime.UtcNow
        };

        await _eventPublisher.PublishAsync(ev, cancellationToken);

        return true;
    }
}
