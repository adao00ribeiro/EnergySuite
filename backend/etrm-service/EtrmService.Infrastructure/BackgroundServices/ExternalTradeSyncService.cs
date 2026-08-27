using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.B2bIntegration.Commands;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EtrmService.Infrastructure.BackgroundServices;

public class ExternalTradeSyncService : BackgroundService
{
    private readonly ILogger<ExternalTradeSyncService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExternalTradeSyncService(ILogger<ExternalTradeSyncService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExternalTradeSyncService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Simulate synchronization every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                
                _logger.LogInformation("Syncing trades from external platforms (BBCE, N5X)...");

                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                // Mock syncing an operation
                var command = new CreateExternalOperationCommand
                {
                    CounterpartyId = Guid.Parse("00000000-0000-0000-0000-000000000002"), // Assume some existing company ID
                    Type = OperationType.Purchase,
                    VolumeMwMed = 15.5m,
                    Price = 250.0m,
                    StartDate = DateTime.UtcNow.Date.AddDays(1),
                    EndDate = DateTime.UtcNow.Date.AddDays(30),
                    ExternalPlatform = "BBCE",
                    ExternalId = Guid.NewGuid().ToString()
                };

                try 
                {
                    var newOperationId = await mediator.Send(command, stoppingToken);
                    _logger.LogInformation($"Successfully synced external trade {command.ExternalId} as Operation {newOperationId}");
                }
                catch (Exception ex)
                {
                    // Might fail if mock Counterparty doesn't exist, ignore for simulation
                    _logger.LogWarning($"Mock sync skipped due to missing test data: {ex.Message}");
                }
            }
            catch (TaskCanceledException)
            {
                break; // Graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in ExternalTradeSyncService.");
            }
        }

        _logger.LogInformation("ExternalTradeSyncService stopping.");
    }
}
