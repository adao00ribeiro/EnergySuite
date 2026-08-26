using MediatR;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Commands;

public class ApplyReadjustmentCommandHandler : IRequestHandler<ApplyReadjustmentCommand, bool>
{
    private readonly IEtrmDbContext _context;

    public ApplyReadjustmentCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApplyReadjustmentCommand request, CancellationToken cancellationToken)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null)
            return false;

        contract.ApplyReadjustment(request.NewPrice, request.Description, request.EffectiveDate);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
