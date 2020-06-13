using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Abstracts.Events
{
    public interface IDomainEventConsumer<in TEvent> : IMessageConsumer<TEvent> where TEvent : IDomainEvent
    {
    }
}
