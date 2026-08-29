using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.B2bIntegration.Commands;
using EtrmService.Application.B2bIntegration.DTOs;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EtrmService.Infrastructure.BackgroundServices;

public class ExternalTradeSyncService : BackgroundService
{
    private readonly ILogger<ExternalTradeSyncService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public ExternalTradeSyncService(ILogger<ExternalTradeSyncService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExternalTradeSyncService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Sincroniza periodicamente a cada 5 minutos.
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                await SyncPendingTradesAsync(stoppingToken);
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

    private async Task SyncPendingTradesAsync(CancellationToken stoppingToken)
    {
        // BK-13: sem URL/credenciais hardcoded. Se não configurado, o sync não ocorre (erro honesto).
        var baseUrl = _configuration["ExternalSync:CceeApiBaseUrl"];
        var apiKey = _configuration["ExternalSync:ApiKey"];
        var tenantValue = _configuration["ExternalSync:TenantId"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("ExternalTradeSync skipped: 'ExternalSync:CceeApiBaseUrl' is not configured.");
            return;
        }

        if (!Guid.TryParse(tenantValue, out var tenantId) || tenantId == Guid.Empty)
        {
            _logger.LogWarning("ExternalTradeSync skipped: 'ExternalSync:TenantId' is not configured or invalid.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var client = httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUrl.TrimEnd('/') + "/trades/sync?status=pending"));
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Add("X-Api-Key", apiKey);

        var response = await client.SendAsync(request, stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"External API returned status: {response.StatusCode}");
            return;
        }

        var tradesJson = await response.Content.ReadAsStringAsync(stoppingToken);

        List<TradeSyncItemDto>? trades;
        try
        {
            trades = JsonSerializer.Deserialize<List<TradeSyncItemDto>>(tradesJson);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize external trade payload (CCEE). Sync stopped for this cycle.");
            return;
        }

        if (trades == null || trades.Count == 0)
        {
            _logger.LogInformation("ExternalTradeSync: no pending trades to process.");
            return;
        }

        foreach (var item in trades)
        {
            if (string.IsNullOrWhiteSpace(item.ExternalId))
            {
                _logger.LogWarning("ExternalTradeSync: trade item without ExternalId ignored.");
                continue;
            }

            if (!TryMapOperationType(item.Type, out var operationType))
            {
                _logger.LogWarning("ExternalTradeSync: unknown operation type '{Type}' for trade {ExternalId}.", item.Type, item.ExternalId);
                continue;
            }

            var command = new CreateExternalOperationCommand
            {
                TenantId = tenantId,
                CounterpartyCode = item.CounterpartyCode,
                Type = operationType,
                VolumeMwMed = item.VolumeMwMed,
                Price = item.Price,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                ExternalPlatform = "CCEE",
                ExternalId = item.ExternalId
            };

            try
            {
                var newOperationId = await mediator.Send(command, stoppingToken);
                _logger.LogInformation("Successfully synced external trade {ExternalId} (CCEE) as Operation {OperationId}", command.ExternalId, newOperationId);
            }
            catch (Exception ex)
            {
                // Regra Sprint 9: contraparte inexistente / falha de integração => rejeita e loga (sem sucesso falso).
                _logger.LogWarning(ex, "Failed to sync external trade {ExternalId} into ETRM: {Reason}", command.ExternalId, ex.Message);
            }
        }
    }

    private static bool TryMapOperationType(string? type, out OperationType operationType)
    {
        operationType = OperationType.Purchase;

        if (string.IsNullOrWhiteSpace(type))
            return false;

        if (type.Equals("PURCHASE", StringComparison.OrdinalIgnoreCase) ||
            type.Equals("BUY", StringComparison.OrdinalIgnoreCase))
        {
            operationType = OperationType.Purchase;
            return true;
        }

        if (type.Equals("SALE", StringComparison.OrdinalIgnoreCase) ||
            type.Equals("SELL", StringComparison.OrdinalIgnoreCase))
        {
            operationType = OperationType.Sale;
            return true;
        }

        return false;
    }
}