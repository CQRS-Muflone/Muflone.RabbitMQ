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

        private List<IMessage> messages = new List<IMessage>();

        public void Register<TMessage, TImplementation>() where TMessage : class, IMessage
            where TImplementation : class, IMessageHandler<TMessage>
        {
            var observed = observers.ContainsKey(typeof(TMessage));
            if (!observed)
                observers.Add(typeof(TMessage), new List<Type> { typeof(TImplementation) });
            else
                observers[typeof(TMessage)].Add(typeof(TImplementation));
        }

        public void RegisterMessage<T>(T message) where T : class, IMessage
        {
            this.messages.Add(message);
        }

        Dictionary<Type, List<Type>> ISubscriberRegistry.Observers => observers;
        public IList<IMessage> Messages => this.messages;

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