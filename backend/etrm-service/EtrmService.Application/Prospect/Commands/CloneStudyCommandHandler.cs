using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Prospect.DTOs;
using EtrmService.Domain.Entities.Prospect;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EtrmService.Application.Prospect.Commands;

public class CloneStudyCommandHandler : IRequestHandler<CloneStudyCommand, StudyDto>
{
    private readonly IEtrmDbContext _context;

    public CloneStudyCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<StudyDto> Handle(CloneStudyCommand request, CancellationToken cancellationToken)
    {
        var originalStudy = await _context.ProspectStudies
            .FirstOrDefaultAsync(s => s.Id == request.StudyId && s.TenantId == request.TenantId, cancellationToken);

        if (originalStudy == null)
            throw new Exception("Original study not found");

        var originalDecks = await _context.ProspectDecks
            .Include(d => d.Versions)
            .Where(d => d.StudyId == originalStudy.Id)
            .ToListAsync(cancellationToken);

        // Clona o estudo
        var newStudy = new Study(
            $"{originalStudy.Name} (Cloned)",
            originalStudy.Description,
            originalStudy.Model,
            originalStudy.StartDate,
            originalStudy.HorizonMonths,
            originalStudy.CreatedBy,
            originalStudy.TenantId);

        _context.ProspectStudies.Add(newStudy);

        // Clona os decks
        foreach (var originalDeck in originalDecks)
        {
            var newDeck = new Deck(
                newStudy.Id,
                originalDeck.Model,
                originalDeck.Period,
                originalDeck.SequenceOrder);

            // Copia apenas a versão 1 (original)
            var originalVersion = originalDeck.Versions.OrderBy(v => v.VersionNumber).FirstOrDefault();
            if (originalVersion != null)
            {
                newDeck.Versions.Add(new DeckVersion(
                    newDeck.Id,
                    1,
                    originalVersion.StoragePath,
                    "Cloned from original"
                ));
            }

            _context.ProspectDecks.Add(newDeck);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new StudyDto
        {
            Id = newStudy.Id,
            Name = newStudy.Name,
            Description = newStudy.Description,
            Model = newStudy.Model,
            StartDate = newStudy.StartDate,
            HorizonMonths = newStudy.HorizonMonths,
            State = newStudy.State.ToString(),
            CreatedAt = newStudy.CreatedAt
        };
    }
}
