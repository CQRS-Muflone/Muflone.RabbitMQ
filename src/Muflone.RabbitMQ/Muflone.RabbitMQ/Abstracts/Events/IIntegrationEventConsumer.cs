using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Abstracts.Events
{
    public interface IIntegrationEventConsumer<in TEvent> where TEvent : IIntegrationEvent
    {
        Task Consume(CancellationToken cancellationToken = default);
    }
}
