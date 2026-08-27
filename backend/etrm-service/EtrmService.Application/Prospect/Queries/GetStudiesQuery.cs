using System;
using System.Collections.Generic;
using EtrmService.Application.Prospect.DTOs;
using MediatR;

namespace EtrmService.Application.Prospect.Queries;

public class GetStudiesQuery : IRequest<List<StudyDto>>
{
    public Guid TenantId { get; set; }
}
