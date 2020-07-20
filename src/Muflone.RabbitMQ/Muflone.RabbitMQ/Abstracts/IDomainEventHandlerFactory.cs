using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface IDomainEventHandlerFactory
    {
        IDomainEventHandler<T> GetDomainEventHandler<T>() where T : class, IDomainEvent;
    }
}