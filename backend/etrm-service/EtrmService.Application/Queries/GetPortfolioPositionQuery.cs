using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Queries.DTOs;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
    private readonly IEtrmDbContext _context;

    public GetPortfolioPositionQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<PortfolioPositionDto> Handle(GetPortfolioPositionQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await _context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.TenantId == request.TenantId, cancellationToken);

        var result = new PortfolioPositionDto
        {
            PortfolioId = request.PortfolioId,
            PortfolioName = portfolio?.Name ?? string.Empty
        };

        var yearStart = new DateTime(request.Year, 1, 1);
        var yearEnd = new DateTime(request.Year, 12, 31, 23, 59, 59);

        var yearOperations = await _context.Operations
            .AsNoTracking()
            .Where(o => o.PortfolioId == request.PortfolioId
                        && o.TenantId == request.TenantId
                        && o.StartDate <= yearEnd
                        && o.EndDate >= yearStart)
            .ToListAsync(cancellationToken);

        for (int month = 1; month <= 12; month++)
        {
            string monthStr = $"{request.Year}-{month:D2}";
            result.Heatmap.XAxisMonths.Add(monthStr);
        }

        decimal totalPurchased = 0;
        decimal totalSold = 0;
        decimal totalResult = 0;

        var monthStartDates = new Dictionary<int, DateTime>();
        var monthEndDates = new Dictionary<int, DateTime>();
        for (int month = 1; month <= 12; month++)
        {
            monthStartDates[month] = new DateTime(request.Year, month, 1);
            monthEndDates[month] = monthStartDates[month].AddMonths(1).AddSeconds(-1);
        }

        foreach (var op in yearOperations)
        {
            var purchased = op.Type == OperationType.Purchase ? op.VolumeMwMed : 0m;
            var sold = op.Type == OperationType.Sale ? op.VolumeMwMed : 0m;

            totalPurchased += purchased;
            totalSold += sold;
            var signedResult = (op.Type == OperationType.Sale ? op.Price : -op.Price) * op.VolumeMwMed;
            totalResult += signedResult;

            for (int month = 1; month <= 12; month++)
            {
                if (op.EndDate < monthStartDates[month] || op.StartDate > monthEndDates[month])
                    continue;

                string monthStr = $"{request.Year}-{month:D2}";

                var monthly = result.MonthlyPositions.FirstOrDefault(m => m.Month == monthStr);
                if (monthly == null)
                {
                    monthly = new MonthlyPositionDto { Month = monthStr };
                    result.MonthlyPositions.Add(monthly);
                }

                monthly.Purchased += purchased;
                monthly.Sold += sold;

                var gap = result.DetailedGaps.FirstOrDefault(g => g.Month == monthStr);
                if (gap == null)
                {
                    gap = new PositionGapDto { Month = monthStr };
                    result.DetailedGaps.Add(gap);
                }

                gap.Purchased += purchased;
                gap.Sold += sold;
            }
        }

        foreach (var monthly in result.MonthlyPositions)
        {
            monthly.Net = Math.Round(monthly.Purchased - monthly.Sold, 2);
        }

        foreach (var gap in result.DetailedGaps)
        {
            gap.NetGap = Math.Round(gap.Purchased - gap.Sold, 2);
        }

        var sortedMonths = result.MonthlyPositions.OrderBy(m => m.Month).ToList();
        result.MonthlyPositions.Clear();
        result.MonthlyPositions.AddRange(sortedMonths);

        result.TotalPurchasedMwMed = Math.Round(totalPurchased, 2);
        result.TotalSoldMwMed = Math.Round(totalSold, 2);
        result.NetPositionMwMed = Math.Round(totalPurchased - totalSold, 2);
        result.EstimatedResult = totalResult;

        return result;
    }
}
