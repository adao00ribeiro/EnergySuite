using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Services;

namespace EtrmService.Application.Portfolios.Queries;

public class GetRankedOpportunitiesQuery : IRequest<List<OpportunityDto>>
{
    public Guid PortfolioId { get; set; }
}

public class GetRankedOpportunitiesQueryHandler : IRequestHandler<GetRankedOpportunitiesQuery, List<OpportunityDto>>
{
    private readonly IOpportunityEngineService _opportunityEngine;

    public GetRankedOpportunitiesQueryHandler(IOpportunityEngineService opportunityEngine)
    {
        _opportunityEngine = opportunityEngine;
    }

    public async Task<List<OpportunityDto>> Handle(GetRankedOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        return await _opportunityEngine.GenerateRankedOpportunitiesAsync(request.PortfolioId);
    }
}
