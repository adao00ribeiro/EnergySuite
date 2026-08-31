using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Pluvia.Queries;

public class GetLatestPrecipitationScenarioQueryHandler : IRequestHandler<GetLatestPrecipitationScenarioQuery, PrecipitationScenario?>
{
    private readonly IEtrmDbContext _context;

    public GetLatestPrecipitationScenarioQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<PrecipitationScenario?> Handle(GetLatestPrecipitationScenarioQuery request, CancellationToken cancellationToken)
    {
        return await _context.PrecipitationScenarios
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
