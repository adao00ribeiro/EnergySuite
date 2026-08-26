using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Pluvia.DTOs;
using EtrmService.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Pluvia.Queries;

public class GetPrecipitationScenariosQueryHandler : IRequestHandler<GetPrecipitationScenariosQuery, IEnumerable<PrecipitationScenarioDto>>
{
    private readonly EtrmDbContext _context;
    private readonly ILogger<GetPrecipitationScenariosQueryHandler> _logger;

    public GetPrecipitationScenariosQueryHandler(EtrmDbContext context, ILogger<GetPrecipitationScenariosQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<PrecipitationScenarioDto>> Handle(GetPrecipitationScenariosQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all precipitation scenarios");

        var scenarios = await _context.PrecipitationScenarios
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new PrecipitationScenarioDto
            {
                Id = s.Id,
                Name = s.Name,
                SourceType = s.SourceType,
                ReferenceDate = s.ReferenceDate,
                HorizonDays = s.HorizonDays,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return scenarios;
    }
}
