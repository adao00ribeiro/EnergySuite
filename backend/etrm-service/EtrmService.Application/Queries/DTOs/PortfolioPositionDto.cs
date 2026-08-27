using System;
using System.Collections.Generic;

namespace EtrmService.Application.Queries.DTOs;

public class PortfolioPositionDto
{
    public Guid PortfolioId { get; set; }
    public string PortfolioName { get; set; } = string.Empty;
    
    public decimal TotalPurchasedMwMed { get; set; }
    public decimal TotalSoldMwMed { get; set; }
    public decimal NetPositionMwMed { get; set; } // Compras - Vendas
    
    public decimal EstimatedResult { get; set; }
    
    public List<MonthlyPositionDto> MonthlyPositions { get; set; } = new();
    
    public List<PositionGapDto> DetailedGaps { get; set; } = new();
    public HeatmapDataDto Heatmap { get; set; } = new();
}

public class MonthlyPositionDto
{
    public string Month { get; set; } = string.Empty; // e.g., "2026-01"
    public decimal Purchased { get; set; }
    public decimal Sold { get; set; }
    public decimal Net { get; set; }
}
