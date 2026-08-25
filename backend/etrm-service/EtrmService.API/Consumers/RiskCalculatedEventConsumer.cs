using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using EtrmService.Application.IntegrationEvents;
using EtrmService.API.Hubs;

namespace EtrmService.API.Consumers
{
    public class RiskCalculatedEventConsumer : IConsumer<RiskCalculatedIntegrationEvent>
    {
        private readonly IHubContext<RiskHub> _hubContext;

        public RiskCalculatedEventConsumer(IHubContext<RiskHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<RiskCalculatedIntegrationEvent> context)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveRiskCalculation", context.Message);
        }
    }
}
