using System;
using System.IO;
using ClosedXML.Excel;

namespace EtrmService.Application.Prospect.Services;

public class ExcelParserService : IExcelParserService
{
    public PremissasDto ParsePremissas(Stream excelStream)
    {
        var dto = new PremissasDto();

        using var workbook = new XLWorkbook(excelStream);
        
        // Assume there is a worksheet called 'Premissas'
        if (!workbook.TryGetWorksheet("Premissas", out var ws))
        {
            throw new ArgumentException("Planilha não contém a aba 'Premissas'.");
        }

        // Extremely simplified parsing logic for demonstration
        dto.GsfScenario = ws.Cell("B2").GetString(); // e.g., "Pessimista"
        
        if (decimal.TryParse(ws.Cell("B3").GetString(), out var demand))
        {
            dto.DemandGrowthPct = demand;
        }

        // Parse initial levels (e.g., SE/CO, S, NE, N)
        for (int row = 6; row <= 9; row++)
        {
            var submarket = ws.Cell(row, 1).GetString();
            if (string.IsNullOrWhiteSpace(submarket)) break;

            if (decimal.TryParse(ws.Cell(row, 2).GetString(), out var level))
            {
                dto.InitialLevels.Add(new ReservoirLevelDto
                {
                    Submarket = submarket,
                    LevelPct = level
                });
            }
        }

        return dto;
    }
}
