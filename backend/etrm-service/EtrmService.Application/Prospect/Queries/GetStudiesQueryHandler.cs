using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Prospect.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Prospect.Queries;

public class GetStudiesQueryHandler : IRequestHandler<GetStudiesQuery, List<StudyDto>>
{
    private readonly IEtrmDbContext _context;

    public GetStudiesQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudyDto>> Handle(GetStudiesQuery request, CancellationToken cancellationToken)
    {
        var studies = await _context.ProspectStudies
            .Where(s => s.TenantId == request.TenantId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StudyDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Model = s.Model,
                StartDate = s.StartDate,
                HorizonMonths = s.HorizonMonths,
                State = s.State.ToString(),
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return studies;
    }
}
