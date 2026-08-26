using System;
using System.Threading.Tasks;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers;

public class EnaCalculatedEventConsumer : IConsumer<EnaCalculatedIntegrationEvent>
{
    private readonly IEtrmDbContext _context;
    private readonly ILogger<EnaCalculatedEventConsumer> _logger;

    public EnaCalculatedEventConsumer(IEtrmDbContext context, ILogger<EnaCalculatedEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EnaCalculatedIntegrationEvent> context)
    {
        var message = context.Message;

        var result = new HydrologicalResult(
            executionId: message.ExecutionId,
            submarket: message.Submarket,
            basin: message.Basin,
            valueMwMed: message.ValueMwMed,
            valuePercentageMlt: message.ValuePercentageMlt,
            targetDate: message.TargetDate
        );

        _context.HydrologicalResults.Add(result);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation($"Saved ENA result for Execution {message.ExecutionId} - {message.Submarket}: {message.ValueMwMed} MWmed ({message.ValuePercentageMlt}%)");
    }
}
