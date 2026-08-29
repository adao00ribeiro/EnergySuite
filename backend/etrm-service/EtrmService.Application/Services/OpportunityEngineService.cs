using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
    private readonly IEtrmDbContext _context;

    public OpportunityEngineService(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<OpportunityDto>> GenerateRankedOpportunitiesAsync(Guid portfolioId)
    {
        var portfolio = await _context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == portfolioId);

        if (portfolio == null)
            return new List<OpportunityDto>();

        var today = DateTime.UtcNow.Date;
        var horizonStart = new DateTime(today.Year, today.Month, 1);
        var horizonEnd = horizonStart.AddMonths(12).AddSeconds(-1);

        var operations = await _context.Operations
            .AsNoTracking()
            .Where(o => o.PortfolioId == portfolioId
                        && o.TenantId == portfolio.TenantId
                        && o.StartDate <= horizonEnd
                        && o.EndDate >= horizonStart)
            .ToListAsync();

        var strategies = await _context.Strategies
            .AsNoTracking()
            .Where(s => s.TenantId == portfolio.TenantId && s.IsActive)
            .ToListAsync();

        var hedgeStrategy = strategies.FirstOrDefault(s => s.Name.Contains("Hedge", StringComparison.OrdinalIgnoreCase));
        var sellStrategy = strategies.FirstOrDefault(s => s.Name.Contains("Excedente", StringComparison.OrdinalIgnoreCase)
                                                         || s.Name.Contains("Venda", StringComparison.OrdinalIgnoreCase));

        var opportunities = new List<OpportunityDto>();

        for (int i = 0; i < 12; i++)
        {
            var monthStart = horizonStart.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);
            var monthKey = monthStart.ToString("yyyy-MM");

            var monthly = operations
                .Where(o => o.StartDate <= monthEnd && o.EndDate >= monthStart)
                .ToList();

            var purchased = monthly.Where(o => o.Type == OperationType.Purchase).Sum(o => o.VolumeMwMed);
            var sold = monthly.Where(o => o.Type == OperationType.Sale).Sum(o => o.VolumeMwMed);
            var net = purchased - sold;

            if (Math.Abs(net) < 0.01m)
                continue;

            if (net < 0)
            {
                var deficitVolume = Math.Abs(net);
                opportunities.Add(new OpportunityDto
                {
                    Id = Guid.NewGuid(),
                    Name = $"Cobertura de Déficit ({monthKey})",
                    Type = "Compra",
                    StrategyName = hedgeStrategy?.Name ?? string.Empty,
                    SuggestedVolumeMwm = Math.Round(deficitVolume, 2),
                    EstimatedSpread = 0m,
                    Score = CalculateScore(deficitVolume),
                    TargetMonth = monthKey,
                    TargetSubmarket = string.Empty
                });
            }
            else
            {
                opportunities.Add(new OpportunityDto
                {
                    Id = Guid.NewGuid(),
                    Name = $"Desova de Excedente ({monthKey})",
                    Type = "Venda",
                    StrategyName = sellStrategy?.Name ?? string.Empty,
                    SuggestedVolumeMwm = Math.Round(net, 2),
                    EstimatedSpread = 0m,
                    Score = CalculateScore(net),
                    TargetMonth = monthKey,
                    TargetSubmarket = string.Empty
                });
            }
        }

        opportunities.Sort((a, b) => b.Score.CompareTo(a.Score)); // Order by Score DESC
        return opportunities;
    }

    private int CalculateScore(decimal volume)
    {
        var magnitude = Math.Abs(volume);
        var score = (int)(magnitude * 5m);
        return Math.Clamp(score, 1, 100);
    }
}
