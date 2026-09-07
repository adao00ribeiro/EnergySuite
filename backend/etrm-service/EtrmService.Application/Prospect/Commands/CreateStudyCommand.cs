using System;
using MediatR;

namespace EtrmService.Application.Prospect.Commands;

public class CreateStudyCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int HorizonMonths { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
}
