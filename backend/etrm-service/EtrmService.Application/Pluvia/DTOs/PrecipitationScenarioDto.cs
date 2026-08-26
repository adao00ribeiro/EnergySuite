using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.Pluvia.DTOs;

public class PrecipitationScenarioDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ScenarioSource SourceType { get; set; }
    public DateTime ReferenceDate { get; set; }
    public int HorizonDays { get; set; }
    public DateTime CreatedAt { get; set; }
}
