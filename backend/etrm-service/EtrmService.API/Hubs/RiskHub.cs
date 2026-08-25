using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace EtrmService.API.Hubs
{
    public class RiskHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
