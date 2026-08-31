using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities.Prospect;
using EtrmService.Domain.Interfaces;
using EtrmService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Infrastructure.Repositories;

public class ProspectRepository : IProspectRepository
{
    private readonly EtrmDbContext _dbContext;

    public ProspectRepository(EtrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Study?> GetStudyByIdAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProspectStudies
            .FirstOrDefaultAsync(s => s.Id == studyId, cancellationToken);
    }

    public async Task<List<Deck>> GetDecksByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProspectDecks
            .Include(d => d.Versions)
            .Where(d => d.StudyId == studyId)
            .OrderBy(d => d.SequenceOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStudyAsync(Study study, CancellationToken cancellationToken = default)
    {
        _dbContext.ProspectStudies.Update(study);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
