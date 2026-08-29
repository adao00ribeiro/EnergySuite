using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Portfolios.Queries;

public class GetRankedOpportunitiesQuery : IRequest<List<OpportunityDto>>
{
    public Guid PortfolioId { get; set; }
}

public class GetRankedOpportunitiesQueryHandler : IRequestHandler<GetRankedOpportunitiesQuery, List<OpportunityDto>>
{
    private readonly IOpportunityEngineService _opportunityEngine;
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetRankedOpportunitiesQueryHandler(IOpportunityEngineService opportunityEngine, IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _opportunityEngine = opportunityEngine;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<OpportunityDto>> Handle(GetRankedOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var portfolioId = request.PortfolioId;
        if (portfolioId == Guid.Empty)
        {
            portfolioId = await _context.Portfolios
                .Where(p => p.TenantId == _currentUserService.TenantId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (portfolioId == Guid.Empty)
                return new List<OpportunityDto>();
        }

        return await _opportunityEngine.GenerateRankedOpportunitiesAsync(portfolioId);
    }
}
