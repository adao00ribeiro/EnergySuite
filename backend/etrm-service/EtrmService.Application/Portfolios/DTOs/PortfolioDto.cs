using System;

namespace EtrmService.Application.Portfolios.DTOs;

public class PortfolioDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Responsible { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
