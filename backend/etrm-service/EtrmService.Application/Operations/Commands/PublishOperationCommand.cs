using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Operations.Commands;

public class PublishOperationCommand : IRequest<bool>
{
    public Guid OperationId { get; set; }
}

public class PublishOperationCommandHandler : IRequestHandler<PublishOperationCommand, bool>
{
    private readonly IEtrmDbContext _context;
    private readonly IEventPublisher _eventPublisher;

    public PublishOperationCommandHandler(IEtrmDbContext context, IEventPublisher eventPublisher)
    {
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(PublishOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _context.Operations
            .FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken);

        if (operation == null)
            return false;

        operation.ChangeState(OperationState.Published);
        await _context.SaveChangesAsync(cancellationToken);

        // Fire integration event for webhooks and other systems
        var integrationEvent = new OperationPublishedIntegrationEvent(
            operation.Id,
            operation.TicketId,
            operation.CounterpartyId,
            operation.State.ToString(),
            operation.VolumeMwMed,
            operation.Price,
            DateTime.UtcNow
        );

        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        return true;
    }
}
