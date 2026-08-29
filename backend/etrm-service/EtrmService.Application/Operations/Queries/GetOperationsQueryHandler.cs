using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Operations.DTOs;

namespace EtrmService.Application.Operations.Queries;

public class GetOperationsQueryHandler : IRequestHandler<GetOperationsQuery, List<OperationDto>>
{
    private readonly IEtrmDbContext _context;

    public GetOperationsQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<OperationDto>> Handle(GetOperationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Operations
            .AsNoTracking()
            .Select(o => new OperationDto
            {
                Id = o.Id,
                TicketId = o.TicketId,
                PortfolioId = o.PortfolioId,
                CounterpartyId = o.CounterpartyId,
                TicketRef = o.Ticket.ReferenceNumber,
                CounterpartyName = o.Counterparty.TradeName != null && o.Counterparty.TradeName != string.Empty
                    ? o.Counterparty.TradeName
                    : o.Counterparty.CorporateName,
                Type = o.Type.ToString(),
                State = o.State.ToString(),
                VolumeMwMed = o.VolumeMwMed,
                Price = o.Price,
                StartDate = o.StartDate,
                EndDate = o.EndDate
            })
            .ToListAsync(cancellationToken);
    }
}
