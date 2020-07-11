using System;
using System.Collections.Generic;
using Muflone.Messages;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface ISubscriberRegistry
    {
        void Register<TMessage, TImplementation>() where TMessage : class, IMessage
            where TImplementation : class, IMessageHandler<TMessage>;
        void RegisterHandlers<TImplementation>(IMessage message)
            where TImplementation : class, IMessageHandler<IMessage>;

        Dictionary<Type, List<Type>> Observers { get; }
        Dictionary<IMessage, List<Type>> Handlers { get; }
    }
}