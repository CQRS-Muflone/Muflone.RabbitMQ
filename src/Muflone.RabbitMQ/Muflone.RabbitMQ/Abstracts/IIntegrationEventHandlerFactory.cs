using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface IIntegrationEventHandlerFactory
    {
        IIntegrationEventHandler<T> GetIntegrationEventHandler<T>() where T : class, IIntegrationEvent;
    }
}