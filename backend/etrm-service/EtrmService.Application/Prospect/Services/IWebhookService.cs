using System.Threading.Tasks;

namespace EtrmService.Application.Prospect.Services;

public interface IWebhookService
{
    Task SendWebhookAsync(string eventName, object payload);
}
