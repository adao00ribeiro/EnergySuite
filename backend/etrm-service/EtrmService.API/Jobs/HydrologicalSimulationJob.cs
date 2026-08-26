using System;
using System.Threading.Tasks;
using EtrmService.Application.Pluvia.Commands;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EtrmService.API.Jobs;

public class HydrologicalSimulationJob : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<HydrologicalSimulationJob> _logger;

    public HydrologicalSimulationJob(IMediator mediator, ILogger<HydrologicalSimulationJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("HydrologicalSimulationJob: Iniciando rotina automática de simulação.");

        try
        {
            var command = new RunHydrologicalSimulationCommand
            {
                ScenarioId = Guid.Empty, // Placeholder: in real scenario we would fetch the default daily scenario
                TargetSubmarket = "ALL"
            };

            var executionId = await _mediator.Send(command);
            
            _logger.LogInformation($"HydrologicalSimulationJob: Simulação automática agendada. ExecutionId: {executionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HydrologicalSimulationJob: Falha ao executar simulação automática.");
        }
    }
}
