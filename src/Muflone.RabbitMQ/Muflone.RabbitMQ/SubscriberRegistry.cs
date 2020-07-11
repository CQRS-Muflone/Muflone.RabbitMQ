using System;
using System.Collections;
using System.Collections.Generic;
using Muflone.Messages;
using Muflone.RabbitMQ.Abstracts;

namespace Muflone.RabbitMQ
{
    public class SubscriberRegistry : ISubscriberRegistry, IEnumerable<KeyValuePair<Type, List<Type>>>
    {
        private readonly Dictionary<Type, List<Type>> observers = new Dictionary<Type, List<Type>>();
        private readonly Dictionary<IMessage, List<Type>> handlers = new Dictionary<IMessage, List<Type>>();

        public void Register<TMessage, TImplementation>() where TMessage : class, IMessage
            where TImplementation : class, IMessageHandler<TMessage>
        {
            var observed = observers.ContainsKey(typeof(TMessage));
            if (!observed)
                observers.Add(typeof(TMessage), new List<Type> { typeof(TImplementation) });
            else
                observers[typeof(TMessage)].Add(typeof(TImplementation));
        }

        public void RegisterHandlers<TImplementation>(IMessage message)
            where TImplementation : class, IMessageHandler<IMessage>
        {
            var handler = this.handlers.ContainsKey(message);
            if (!handler)
                handlers.Add(message, new List<Type>{typeof(TImplementation)});
            else
                handlers[message].Add(typeof(TImplementation));
        }

        Dictionary<Type, List<Type>> ISubscriberRegistry.Observers => observers;
        public Dictionary<IMessage, List<Type>> Handlers => this.handlers;

        public IEnumerable<Type> Get<TMessage>() where TMessage : class, IMessage
        {
            var observed = observers.ContainsKey(typeof(TMessage));
            return observed ? observers[typeof(TMessage)] : new List<Type>();
        }

        public IEnumerator<KeyValuePair<Type, List<Type>>> GetEnumerator()
        {
            return observers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}