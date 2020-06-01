using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events
{
    public interface IDomainEventConsumer<in TEvent> where TEvent : IDomainEvent
    {
        Task Publish(TEvent @event, CancellationToken cancellationToken = default);
    }
}