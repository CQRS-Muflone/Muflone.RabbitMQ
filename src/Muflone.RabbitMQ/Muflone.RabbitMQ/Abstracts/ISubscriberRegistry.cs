using System;
using System.Collections.Generic;
using Muflone.Messages;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface ISubscriberRegistry
    {
        void Register<TMessage, TImplementation>() where TMessage : class, IMessage
            where TImplementation : class, IMessageHandler<TMessage>;
        void RegisterMessage<T>(T message) where T : class, IMessage;

        IEnumerable<Type> Get<TMessage>() where TMessage : class, IMessage;

        Dictionary<Type, List<Type>> Observers { get; }
        IList<IMessage> Messages { get; }
    }
}