using System;
using MediatR;

namespace EtrmService.Application.Prospect.Commands;

public class ExecuteStudyCommand : IRequest<bool>
{
    public Guid StudyId { get; set; }
    public Guid TenantId { get; set; }
}
