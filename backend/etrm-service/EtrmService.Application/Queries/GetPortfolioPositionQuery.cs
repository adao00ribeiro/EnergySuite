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

    public GetPortfolioPositionQuery(Guid portfolioId, Guid tenantId, int year)
    {
        PortfolioId = portfolioId;
        TenantId = tenantId;
        Year = year;
    }
}

public class GetPortfolioPositionQueryHandler : IRequestHandler<GetPortfolioPositionQuery, PortfolioPositionDto>
{
    public async Task<PortfolioPositionDto> Handle(GetPortfolioPositionQuery request, CancellationToken cancellationToken)
    {
        // Mock data for Sprint 1 Menza UI development
        await Task.Delay(100, cancellationToken); // Simulate DB delay

        var result = new PortfolioPositionDto
        {
            PortfolioId = request.PortfolioId,
            PortfolioName = "Portfólio Principal (Mock)",
            TotalPurchasedMwMed = 150.5m,
            TotalSoldMwMed = 120.0m,
            NetPositionMwMed = 30.5m,
            EstimatedResult = 450000.00m,
            MonthlyPositions = new List<MonthlyPositionDto>()
        };

        var random = new Random(request.PortfolioId.GetHashCode());

        for (int month = 1; month <= 12; month++)
        {
            var purchased = 100m + (decimal)(random.NextDouble() * 50);
            var sold = 90m + (decimal)(random.NextDouble() * 60);
            
            result.MonthlyPositions.Add(new MonthlyPositionDto
            {
                Month = $"{request.Year}-{month:D2}",
                Purchased = Math.Round(purchased, 2),
                Sold = Math.Round(sold, 2),
                Net = Math.Round(purchased - sold, 2)
            });
        }

        return result;
    }
}
