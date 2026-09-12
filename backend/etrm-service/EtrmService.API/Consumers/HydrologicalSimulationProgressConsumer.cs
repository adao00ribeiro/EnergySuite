using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using EtrmService.API.Hubs;
using EtrmService.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers
{
    public class HydrologicalSimulationProgressConsumer : IConsumer<HydrologicalSimulationProgressEvent>
    {
        private readonly IHubContext<EtrmHub, IEtrmClient> _hubContext;
        private readonly ILogger<HydrologicalSimulationProgressConsumer> _logger;

        public HydrologicalSimulationProgressConsumer(
            IHubContext<EtrmHub, IEtrmClient> hubContext,
            ILogger<HydrologicalSimulationProgressConsumer> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<HydrologicalSimulationProgressEvent> context)
        {
            var evt = context.Message;
            _logger.LogInformation("Received HydrologicalSimulationProgressEvent: SimulationId={SimulationId}, Progress={Percentage}%, TenantId={TenantId}",
                evt.SimulationId, evt.Percentage, evt.TenantId);

            var payload = new
            {
                simulationId = evt.SimulationId,
                tenantId = evt.TenantId,
                percentage = evt.Percentage,
                status = evt.Status,
                message = evt.Message,
                timestamp = evt.Timestamp
            };

            if (!string.IsNullOrEmpty(evt.TenantId))
            {
                await _hubContext.Clients.Group($"Tenant_{evt.TenantId}").ReceiveSimulationProgress(payload);
            }
            
            // Broadcast fallback to all clients for monitoring dashboards
            await _hubContext.Clients.All.ReceiveSimulationProgress(payload);
        }
    }
}
