using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using MediatR;

namespace EtrmService.Application.Pluvia.Commands;

public class BlendCustomMapCommandHandler : IRequestHandler<BlendCustomMapCommand, Guid>
{
    private readonly IEtrmDbContext _context;

    public BlendCustomMapCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(BlendCustomMapCommand request, CancellationToken cancellationToken)
    {
        var customScenario = new CustomScenario(
            name: request.Name,
            referenceDate: request.ReferenceDate,
            horizonDays: request.HorizonDays,
            uploadUrl: null,
            blendConfig: request.BlendConfig
        );

        _context.CustomScenarios.Add(customScenario);
        await _context.SaveChangesAsync(cancellationToken);

        return customScenario.Id;
    }
}
