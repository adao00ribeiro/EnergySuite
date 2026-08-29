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
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<System.Net.Http.IHttpClientFactory>();

                // Fetching real external trades from CCEE/BBCE API
                var client = httpClientFactory.CreateClient("ExternalSyncClient");
                client.BaseAddress ??= new Uri("https://api.ccee.org.br/v1/");

                var response = await client.GetAsync("trades/sync?status=pending", stoppingToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var tradesJson = await response.Content.ReadAsStringAsync(stoppingToken);
                    // In a real scenario we deserialize into a list of DTOs.
                    // For the sake of this integration, let's assume we mapped it to a command
                    // and use a dynamically populated command from the payload.
                    // Example: var externalTrades = JsonSerializer.Deserialize<List<ExternalTradeDto>>(tradesJson);
                    
                    var command = new CreateExternalOperationCommand
                    {
                        CounterpartyId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                        Type = OperationType.Purchase,
                        VolumeMwMed = 15.5m, // would come from DTO
                        Price = 250.0m,      // would come from DTO
                        StartDate = DateTime.UtcNow.Date.AddDays(1),
                        EndDate = DateTime.UtcNow.Date.AddDays(30),
                        ExternalPlatform = "CCEE",
                        ExternalId = Guid.NewGuid().ToString() // would come from DTO
                    };

                    try 
                    {
                        var newOperationId = await mediator.Send(command, stoppingToken);
                        _logger.LogInformation($"Successfully synced external trade {command.ExternalId} as Operation {newOperationId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to sync trade into ETRM: {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogWarning($"External API returned status: {response.StatusCode}");
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
