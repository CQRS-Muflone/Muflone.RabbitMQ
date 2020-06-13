using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Abstracts.Events
{
    public interface IIntegrationEventConsumer<in TEvent> : IMessageConsumer<TEvent> where TEvent : IIntegrationEvent
    {
    }
}
