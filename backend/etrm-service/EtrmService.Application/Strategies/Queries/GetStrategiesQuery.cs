using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Strategies.Queries;

public class StrategyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Draft", "Approved", "Inactive"
}

public class GetStrategiesQuery : IRequest<List<StrategyDto>>
{
    public Guid TenantId { get; set; }
}

public class GetStrategiesQueryHandler : IRequestHandler<GetStrategiesQuery, List<StrategyDto>>
{
    private readonly IEtrmDbContext _context;

    public GetStrategiesQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<StrategyDto>> Handle(GetStrategiesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Strategies
            .AsNoTracking()
            .Where(s => s.TenantId == request.TenantId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StrategyDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Status = s.Status
            })
            .ToListAsync(cancellationToken);
    }
}
