using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EtrmService.API.Hubs;

public class ProspectHub : Hub
{
    public async Task SubscribeToStudy(Guid studyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Study_{studyId}");
        await Clients.Caller.SendAsync("LogReceived", $"[SYSTEM] Conectado aos logs do Estudo {studyId}.");
    }

    public async Task UnsubscribeFromStudy(Guid studyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Study_{studyId}");
    }
}
