using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EtrmService.Application.Interfaces;
using MassTransit;

namespace EtrmService.Infrastructure.Messaging
{
    public class KafkaEventPublisher : IEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;

        public KafkaEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
        {
            var producer = _serviceProvider.GetRequiredService<ITopicProducer<T>>();
            await producer.Produce(@event, cancellationToken);
        }
    }
}
