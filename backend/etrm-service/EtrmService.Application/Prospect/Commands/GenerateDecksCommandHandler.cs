using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities.Prospect;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Prospect.Commands;

public class GenerateDecksCommandHandler : IRequestHandler<GenerateDecksCommand, bool>
{
    private readonly IEtrmDbContext _context;

    public GenerateDecksCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(GenerateDecksCommand request, CancellationToken cancellationToken)
    {
        var study = await _context.ProspectStudies
            .FirstOrDefaultAsync(s => s.Id == request.StudyId && s.TenantId == request.TenantId, cancellationToken);

        if (study == null)
            throw new Exception("Study not found");

        var currentPeriod = study.StartDate;

        for (int i = 1; i <= study.HorizonMonths; i++)
        {
            var deck = new Deck(study.Id, study.Model, currentPeriod, i);
            
            // Add a mock vazoes.dat version to represent the base file cloning
            deck.Versions.Add(new DeckVersion(deck.Id, 1, $"/s3/bucket/study/{study.Id}/deck_{i}/vazoes.dat", "Initial generation"));

            _context.ProspectDecks.Add(deck);

            // Increment one month (assuming monthly models like NEWAVE, adjust if weekly like DECOMP)
            currentPeriod = currentPeriod.AddMonths(1);
        }

        study.ChangeState(Domain.Enums.StudyState.Generated);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
