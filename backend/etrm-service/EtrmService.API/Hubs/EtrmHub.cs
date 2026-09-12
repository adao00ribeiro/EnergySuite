using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Hubs
{
    public interface IEtrmClient
    {
        Task ReceiveSimulationProgress(object progressData);
        Task ReceiveRiskUpdate(object riskData);
        Task ReceiveNotification(string title, string message);
    }

    public class EtrmHub : Hub<IEtrmClient>
    {
        private readonly ILogger<EtrmHub> _logger;

        public EtrmHub(ILogger<EtrmHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("SignalR Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            _logger.LogInformation("SignalR Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinTenantGroup(string tenantId)
        {
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
                _logger.LogInformation("Connection {ConnectionId} joined group Tenant_{TenantId}", Context.ConnectionId, tenantId);
            }
        }

        public async Task LeaveTenantGroup(string tenantId)
        {
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
                _logger.LogInformation("Connection {ConnectionId} left group Tenant_{TenantId}", Context.ConnectionId, tenantId);
            }
        }
    }
}
