using System;
using System.Collections.Generic;

namespace EtrmService.Application.Queries.DTOs;

public class ContractAmendmentDto
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public decimal PreviousPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}

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
    public int Version { get; set; }
    public string PriceIndexType { get; set; } = string.Empty;
    public decimal FlexibilityMargin { get; set; }
    public List<ContractAmendmentDto> Amendments { get; set; } = new();
}
