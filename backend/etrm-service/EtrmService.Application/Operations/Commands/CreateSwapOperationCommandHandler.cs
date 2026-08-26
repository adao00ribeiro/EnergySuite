using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;

namespace EtrmService.Application.Operations.Commands;

public class CreateSwapOperationCommandHandler : IRequestHandler<CreateSwapOperationCommand, (Guid LegAId, Guid LegBId)>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateSwapOperationCommandHandler(IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<(Guid LegAId, Guid LegBId)> Handle(CreateSwapOperationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        // Generates the linked swap pair (one purchase, one sale)
        var (legA, legB) = Operation.CreateSwapPair(
            request.TicketId,
            request.PortfolioId,
            request.CounterpartyId,
            request.VolumeMwMed,
            request.Price,
            request.StartDate,
            request.EndDate,
            tenantId
        );

        _context.Operations.Add(legA);
        _context.Operations.Add(legB);

        await _context.SaveChangesAsync(cancellationToken);

        return (legA.Id, legB.Id);
    }
}
