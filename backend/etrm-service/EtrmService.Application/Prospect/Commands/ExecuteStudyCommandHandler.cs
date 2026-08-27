using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Prospect.Events;
using EtrmService.Application.Prospect.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Prospect.Commands;

public class ExecuteStudyCommandHandler : IRequestHandler<ExecuteStudyCommand, bool>
{
    private readonly IEtrmDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    private readonly IWebhookService _webhookService;

    public ExecuteStudyCommandHandler(IEtrmDbContext context, IEventPublisher eventPublisher, IWebhookService webhookService)
    {
        _context = context;
        _eventPublisher = eventPublisher;
        _webhookService = webhookService;
    }

    public async Task<bool> Handle(ExecuteStudyCommand request, CancellationToken cancellationToken)
    {
        var study = await _context.ProspectStudies.FirstOrDefaultAsync(s => s.Id == request.StudyId, cancellationToken);
        
        if (study == null)
            throw new Exception("Study not found");

        study.ChangeState(Domain.Enums.StudyState.Queued);
        await _context.SaveChangesAsync(cancellationToken);

        var ev = new StudyExecutionRequestedEvent
        {
            StudyId = study.Id,
            TenantId = study.TenantId,
            RequestedAt = DateTime.UtcNow
        };

        await _eventPublisher.PublishAsync(ev, cancellationToken);
        await _webhookService.SendWebhookAsync("study.queued", new { StudyId = study.Id, Status = "Queued" });

        return true;
    }
}
