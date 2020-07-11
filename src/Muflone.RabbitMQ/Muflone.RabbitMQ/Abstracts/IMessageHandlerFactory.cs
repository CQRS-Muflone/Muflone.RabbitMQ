using System;
using Muflone.Messages;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler GetMessageHandler(Type handlerType);
    }
}