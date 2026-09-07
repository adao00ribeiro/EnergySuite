using System;

namespace EtrmService.Application.Prospect.DTOs;

public class StudyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int HorizonMonths { get; set; }
    public string State { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
