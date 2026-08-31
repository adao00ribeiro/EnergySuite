using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities.Prospect;

namespace EtrmService.Domain.Interfaces;

public interface IProspectRepository
{
    Task<Study?> GetStudyByIdAsync(Guid studyId, CancellationToken cancellationToken = default);
    Task<List<Deck>> GetDecksByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default);
    Task UpdateStudyAsync(Study study, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
