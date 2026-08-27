using System;
using MediatR;
using EtrmService.Application.Prospect.DTOs;

namespace EtrmService.Application.Prospect.Commands;

public class CloneStudyCommand : IRequest<StudyDto>
{
    public Guid StudyId { get; set; }
    public Guid TenantId { get; set; }
}
