using System;
using System.Collections.Generic;

namespace EtrmService.Application.Queries.DTOs;

public class PositionGapDto
{
    public string Month { get; set; } = string.Empty;
    public string Submarket { get; set; } = string.Empty;
    public string EnergySource { get; set; } = string.Empty;
    
    public decimal Purchased { get; set; }
    public decimal Sold { get; set; }
    public decimal NetGap { get; set; }
    
    public bool IsDeficit => NetGap < 0;
}

public class HeatmapDataDto
{
    public List<HeatmapPointDto> Points { get; set; } = new();
    public List<string> XAxisMonths { get; set; } = new();
    public List<string> YAxisSubmarkets { get; set; } = new();
}

public class HeatmapPointDto
{
    public int XIndex { get; set; }
    public int YIndex { get; set; }
    public decimal GapValue { get; set; }
}
