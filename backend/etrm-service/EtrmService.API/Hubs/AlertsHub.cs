using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace EtrmService.API.Hubs
{
    public class AlertsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SystemAlerts");
            await Groups.AddToGroupAsync(Context.ConnectionId, "RiskAlerts");
            await Groups.AddToGroupAsync(Context.ConnectionId, "OperationalAlerts");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SystemAlerts");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "RiskAlerts");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "OperationalAlerts");
            await base.OnDisconnectedAsync(exception);
        }

        // Example method to be called by backend services when Kafka messages arrive
        public async Task SendAlert(string category, string severity, string title, string message)
        {
            var alert = new
            {
                id = Guid.NewGuid().ToString(),
                category,
                severity,
                title,
                message,
                timestamp = DateTime.UtcNow,
                read = false
            };

            await Clients.Group($"{category}Alerts").SendAsync("ReceiveAlert", alert);
        }
    }
}
