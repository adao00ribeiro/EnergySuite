using System.Collections.Generic;
using System.IO;

namespace EtrmService.Application.Prospect.Services;

public class PremissasDto
{
    public string GsfScenario { get; set; } = string.Empty;
    public decimal DemandGrowthPct { get; set; }
    public List<ReservoirLevelDto> InitialLevels { get; set; } = new();
}

public class ReservoirLevelDto
{
    public string Submarket { get; set; } = string.Empty;
    public decimal LevelPct { get; set; }
}

public interface IExcelParserService
{
    PremissasDto ParsePremissas(Stream excelStream);
}
