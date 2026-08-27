using System;
using EtrmService.Application.Prospect.DTOs;
using MediatR;

namespace EtrmService.Application.Prospect.Queries;

public class GetStudyResultsQuery : IRequest<StudyResultResponseDto>
{
    public Guid StudyId { get; set; }
    public Guid TenantId { get; set; }
}
