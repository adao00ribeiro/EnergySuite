using System;
using EtrmService.Domain.Enums;
using MediatR;

namespace EtrmService.Application.Pluvia.Commands;

public class CreatePrecipitationScenarioCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public ScenarioSource SourceType { get; set; }
    public DateTime ReferenceDate { get; set; }
    public int HorizonDays { get; set; }
}
