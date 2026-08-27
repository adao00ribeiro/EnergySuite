using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtrmService.Application.Services;

public class OpportunityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Compra" ou "Venda"
    public string StrategyName { get; set; } = string.Empty;
    public decimal SuggestedVolumeMwm { get; set; }
    public decimal EstimatedSpread { get; set; }
    public int Score { get; set; }
    public string TargetMonth { get; set; } = string.Empty;
    public string TargetSubmarket { get; set; } = string.Empty;
}

public interface IOpportunityEngineService
{
    Task<List<OpportunityDto>> GenerateRankedOpportunitiesAsync(Guid portfolioId);
}

public class OpportunityEngineService : IOpportunityEngineService
{
    public async Task<List<OpportunityDto>> GenerateRankedOpportunitiesAsync(Guid portfolioId)
    {
        await Task.Delay(50); // Simulate processing

        // Em uma implementação real, este motor buscaria os "Gaps" do portfólio 
        // e cruzaria com as Estratégias ativas para gerar Oportunidades viáveis.
        var opportunities = new List<OpportunityDto>
        {
            new OpportunityDto 
            { 
                Id = Guid.NewGuid(), 
                Name = "Cobertura Déficit SE/CO (Julho)", 
                Type = "Compra", 
                StrategyName = "Hedge de Inverno", 
                SuggestedVolumeMwm = 15.5m, 
                EstimatedSpread = 12.0m, // Spread negativo/custo evitado
                Score = 95, 
                TargetMonth = "2026-07", 
                TargetSubmarket = "SE/CO" 
            },
            new OpportunityDto 
            { 
                Id = Guid.NewGuid(), 
                Name = "Desova de Excedente (Eólico NE)", 
                Type = "Venda", 
                StrategyName = "Venda Excedente Eólica", 
                SuggestedVolumeMwm = 22.0m, 
                EstimatedSpread = 45.0m, // Lucro por MWh
                Score = 88, 
                TargetMonth = "2026-10", 
                TargetSubmarket = "NE" 
            },
            new OpportunityDto 
            { 
                Id = Guid.NewGuid(), 
                Name = "Arbitragem Estrutural", 
                Type = "Compra", 
                StrategyName = "Arbitragem Sul x SE", 
                SuggestedVolumeMwm = 10.0m, 
                EstimatedSpread = 25.5m, 
                Score = 72, 
                TargetMonth = "2026-11", 
                TargetSubmarket = "SUL" 
            }
        };

        opportunities.Sort((a, b) => b.Score.CompareTo(a.Score)); // Order by Score DESC

        return opportunities;
    }
}
