using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;

namespace EtrmService.Application.Tickets.Commands;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Guid>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateTicketCommandHandler(IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = new Ticket(request.ReferenceNumber, _currentUserService.TenantId);
        
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        return ticket.Id;
    }
}
