using System;
using System.Linq;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Pluvia.Commands;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EtrmService.API.Jobs;

public class HydrologicalSimulationJob : IJob
{
    private readonly IMediator _mediator;
    private readonly IEtrmDbContext _context;
    private readonly ILogger<HydrologicalSimulationJob> _logger;

    public HydrologicalSimulationJob(IMediator mediator, IEtrmDbContext context, ILogger<HydrologicalSimulationJob> logger)
    {
        _mediator = mediator;
        _context = context;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("HydrologicalSimulationJob: Iniciando rotina automática de simulação.");

        try
        {
            // O modelo de PrecipitationScenario não possui um marcador "IsDefault". 
            // O cenário hidrológico padrão do dia é representado pelo mais recente criado.
            var scenario = await _context.PrecipitationScenarios
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(context.CancellationToken);

            if (scenario == null)
            {
                _logger.LogWarning("HydrologicalSimulationJob: Nenhum cenário de precipitação encontrado. Simulação cancelada.");
                return;
            }

            var command = new RunHydrologicalSimulationCommand
            {
                ScenarioId = scenario.Id,
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
