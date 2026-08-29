using System;
using System.Collections.Generic;
using System.Text.Json;
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

        if (string.IsNullOrWhiteSpace(study.ResultsJson))
            return response;

        try
        {
            var results = JsonSerializer.Deserialize<List<StudyResultDto>>(study.ResultsJson);
            if (results != null)
                response.Results.AddRange(results);
        }
        catch (JsonException)
        {
            return response;
        }

        return response;
    }
}
