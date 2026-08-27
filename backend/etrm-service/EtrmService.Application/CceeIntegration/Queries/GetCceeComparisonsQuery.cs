using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.CceeIntegration.Queries;

public class CceeComparisonDto
{
    public Guid Id { get; set; }
    public Guid? OperationId { get; set; }
    public string CounterpartyCceeCode { get; set; }
    public DateTime Period { get; set; }
    public decimal BackOpsVolume { get; set; }
    public decimal CceeVolume { get; set; }
    public decimal Difference { get; set; }
    public string Status { get; set; }
}

public class GetCceeComparisonsQuery : IRequest<List<CceeComparisonDto>>
{
}

public class GetCceeComparisonsQueryHandler : IRequestHandler<GetCceeComparisonsQuery, List<CceeComparisonDto>>
{
    private readonly IEtrmDbContext _context;

    public GetCceeComparisonsQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<CceeComparisonDto>> Handle(GetCceeComparisonsQuery request, CancellationToken cancellationToken)
    {
        var comparisons = await _context.CceeComparisons
            .OrderByDescending(c => c.CreatedAt)
            .Take(100) // limit for demo purposes
            .ToListAsync(cancellationToken);

        return comparisons.Select(c => new CceeComparisonDto
        {
            Id = c.Id,
            OperationId = c.OperationId,
            CounterpartyCceeCode = c.CounterpartyCceeCode,
            Period = c.Period,
            BackOpsVolume = c.BackOpsVolume,
            CceeVolume = c.CceeVolume,
            Difference = c.Difference,
            Status = c.Status.ToString()
        }).ToList();
    }
}
