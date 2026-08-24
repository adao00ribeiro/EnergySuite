using System;

namespace EtrmService.Application.Queries.DTOs;

public class ContractDto
{
    public Guid Id { get; set; }
    public string CounterpartyName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Submarket { get; set; } = string.Empty;
    public decimal VolumeMwMed { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
