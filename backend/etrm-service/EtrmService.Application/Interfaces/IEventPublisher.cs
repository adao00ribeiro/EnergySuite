using System.Threading;
using System.Threading.Tasks;

namespace EtrmService.Application.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
    }
}
