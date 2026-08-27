using System;

namespace EtrmService.Application.Prospect.DTOs;

public class StudyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Model { get; set; }
    public DateTime StartDate { get; set; }
    public int HorizonMonths { get; set; }
    public string State { get; set; }
    public DateTime CreatedAt { get; set; }
}
