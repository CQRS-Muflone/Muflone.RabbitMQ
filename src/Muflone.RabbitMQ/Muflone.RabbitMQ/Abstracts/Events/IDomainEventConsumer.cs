using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Abstracts.Events
{
    public interface IDomainEventConsumer<in TEvent> where TEvent : IDomainEvent
    {
        Task Publish(TEvent @event, CancellationToken cancellationToken = default);
    }
}
