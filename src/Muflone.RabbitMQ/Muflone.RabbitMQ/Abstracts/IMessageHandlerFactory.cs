using Muflone.Messages;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler<T> GetMessageHandler<T>() where T : class, IMessage;
    }
}