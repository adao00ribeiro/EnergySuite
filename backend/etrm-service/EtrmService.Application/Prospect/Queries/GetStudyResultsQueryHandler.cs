using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Prospect.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Prospect.Queries;

public class GetStudyResultsQueryHandler : IRequestHandler<GetStudyResultsQuery, StudyResultResponseDto>
{
    private readonly IEtrmDbContext _context;

    public GetStudyResultsQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<StudyResultResponseDto> Handle(GetStudyResultsQuery request, CancellationToken cancellationToken)
    {
        var study = await _context.ProspectStudies
            .FirstOrDefaultAsync(s => s.Id == request.StudyId && s.TenantId == request.TenantId, cancellationToken);

        if (study == null)
            throw new Exception("Study not found");

        var response = new StudyResultResponseDto();

        var random = new Random(request.StudyId.GetHashCode());

        var currentPeriod = study.StartDate;
        for (int i = 1; i <= study.HorizonMonths; i++)
        {
            // Gerando PLD fictício (entre 60 e 200) simulando a saída de um DECOMP/NEWAVE
            response.Results.Add(new StudyResultDto
            {
                Month = currentPeriod.ToString("MM/yyyy"),
                PldSE = Math.Round((decimal)(60 + random.NextDouble() * 100), 2),
                PldS = Math.Round((decimal)(50 + random.NextDouble() * 120), 2),
                PldNE = Math.Round((decimal)(70 + random.NextDouble() * 80), 2),
                PldN = Math.Round((decimal)(60 + random.NextDouble() * 50), 2),
            });

            currentPeriod = currentPeriod.AddMonths(1);
        }

        return response;
    }
}
