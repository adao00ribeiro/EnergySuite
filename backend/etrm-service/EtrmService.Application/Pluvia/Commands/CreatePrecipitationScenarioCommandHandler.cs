using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;
using EtrmService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Pluvia.Commands;

public class CreatePrecipitationScenarioCommandHandler : IRequestHandler<CreatePrecipitationScenarioCommand, Guid>
{
    private readonly IEtrmDbContext _context;
    private readonly ILogger<CreatePrecipitationScenarioCommandHandler> _logger;

    public CreatePrecipitationScenarioCommandHandler(IEtrmDbContext context, ILogger<CreatePrecipitationScenarioCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreatePrecipitationScenarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new precipitation scenario {ScenarioName} from source {SourceType}", request.Name, request.SourceType);

        var scenario = new PrecipitationScenario(
            request.Name,
            request.SourceType,
            request.ReferenceDate,
            request.HorizonDays
        );

        _context.PrecipitationScenarios.Add(scenario);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Precipitation scenario {ScenarioId} successfully created.", scenario.Id);

        return scenario.Id;
    }
}
