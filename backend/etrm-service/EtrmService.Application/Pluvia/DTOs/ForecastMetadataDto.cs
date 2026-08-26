using System;

namespace EtrmService.Application.Pluvia.DTOs;

public class ForecastMetadataDto
{
    public Guid Id { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public DateTime ReferenceDate { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public int EnsembleMembers { get; set; }
    public string LakehousePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
