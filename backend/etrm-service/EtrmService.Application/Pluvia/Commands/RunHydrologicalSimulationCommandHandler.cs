using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Pluvia.Commands;

public class RunHydrologicalSimulationCommandHandler : IRequestHandler<RunHydrologicalSimulationCommand, Guid>
{
    private readonly IEtrmDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RunHydrologicalSimulationCommandHandler> _logger;

    public RunHydrologicalSimulationCommandHandler(
        IEtrmDbContext context, 
        IEventPublisher eventPublisher, 
        ILogger<RunHydrologicalSimulationCommandHandler> logger)
    {
        _context = context;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Guid> Handle(RunHydrologicalSimulationCommand request, CancellationToken cancellationToken)
    {
        var scenario = await _context.PrecipitationScenarios
            .FirstOrDefaultAsync(s => s.Id == request.ScenarioId, cancellationToken);

        if (scenario == null)
            throw new Exception("Scenario not found");

        var execution = new ModelExecution(scenario.Id, EtrmService.Domain.Enums.HydrologicalModelType.Smap);

        _context.ModelExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Disparando evento de simulação no Kafka para o cenário {scenario.Name}");

        var integrationEvent = new SimulationRequestedIntegrationEvent
        {
            SimulationId = execution.Id,
            ScenarioId = scenario.Id,
            ModelName = "SMAP", // Default
            TargetSubmarket = request.TargetSubmarket
        };

        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        return execution.Id;
    }
}
