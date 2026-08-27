using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Queries.DTOs;
using MediatR;

namespace EtrmService.Application.Queries;

public class GetPortfolioPositionQuery : IRequest<PortfolioPositionDto>
{
    public Guid PortfolioId { get; set; }
    public Guid TenantId { get; set; }
    public int Year { get; set; }
    public string? Submarket { get; set; }
    public string? EnergySource { get; set; }

    public GetPortfolioPositionQuery(Guid portfolioId, Guid tenantId, int year, string? submarket = null, string? energySource = null)
    {
        PortfolioId = portfolioId;
        TenantId = tenantId;
        Year = year;
        Submarket = submarket;
        EnergySource = energySource;
    }
}

public class GetPortfolioPositionQueryHandler : IRequestHandler<GetPortfolioPositionQuery, PortfolioPositionDto>
{
    public async Task<PortfolioPositionDto> Handle(GetPortfolioPositionQuery request, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken); // Simula delay do banco

        var result = new PortfolioPositionDto
        {
            PortfolioId = request.PortfolioId,
            PortfolioName = "Portfólio Principal (Mock Sprint 2)",
            TotalPurchasedMwMed = 150.5m,
            TotalSoldMwMed = 120.0m,
            NetPositionMwMed = 30.5m,
            EstimatedResult = 450000.00m
        };

        var random = new Random(request.PortfolioId.GetHashCode());
        var submarkets = new[] { "SE/CO", "SUL", "NE", "NORTE" };

        result.Heatmap.YAxisSubmarkets.AddRange(submarkets);

        for (int month = 1; month <= 12; month++)
        {
            string monthStr = $"{request.Year}-{month:D2}";
            result.Heatmap.XAxisMonths.Add(monthStr);

            decimal monthPurchased = 0;
            decimal monthSold = 0;

            for (int s = 0; s < submarkets.Length; s++)
            {
                var purchased = (decimal)(random.NextDouble() * 30);
                var sold = (decimal)(random.NextDouble() * 35); // Leve tendência a déficit em alguns
                var net = Math.Round(purchased - sold, 2);

                monthPurchased += purchased;
                monthSold += sold;

                result.DetailedGaps.Add(new PositionGapDto
                {
                    Month = monthStr,
                    Submarket = submarkets[s],
                    EnergySource = "Convencional",
                    Purchased = Math.Round(purchased, 2),
                    Sold = Math.Round(sold, 2),
                    NetGap = net
                });

                result.Heatmap.Points.Add(new HeatmapPointDto
                {
                    XIndex = month - 1,
                    YIndex = s,
                    GapValue = net
                });
            }

            result.MonthlyPositions.Add(new MonthlyPositionDto
            {
                Month = monthStr,
                Purchased = Math.Round(monthPurchased, 2),
                Sold = Math.Round(monthSold, 2),
                Net = Math.Round(monthPurchased - monthSold, 2)
            });
        }

        return result;
    }
}
