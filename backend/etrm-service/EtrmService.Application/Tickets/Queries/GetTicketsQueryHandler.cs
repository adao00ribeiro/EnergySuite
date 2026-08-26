using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Tickets.DTOs;

namespace EtrmService.Application.Tickets.Queries;

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, List<TicketDto>>
{
    private readonly IEtrmDbContext _context;

    public GetTicketsQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Select(t => new TicketDto
            {
                Id = t.Id,
                ReferenceNumber = t.ReferenceNumber,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
