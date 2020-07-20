using System;
using System.Collections;
using System.Collections.Generic;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Abstracts;

namespace Muflone.RabbitMQ
{
    public class SubscriberRegistry : ISubscriberRegistry, IEnumerable<KeyValuePair<Type, List<Type>>>
    {
        private readonly Dictionary<Type, List<Type>> observers = new Dictionary<Type, List<Type>>();
        private readonly Dictionary<Type, List<Type>> commandObservers = new Dictionary<Type, List<Type>>();
        private readonly Dictionary<Type, List<Type>> domainEventObservers = new Dictionary<Type, List<Type>>();
        private readonly Dictionary<Type, List<Type>> integrationEventObservers = new Dictionary<Type, List<Type>>();

        private readonly Dictionary<ICommand, List<Type>> commands =new Dictionary<ICommand, List<Type>>();

        Dictionary<Type, List<Type>> ISubscriberRegistry.Observers => observers;
        Dictionary<Type, List<Type>> ISubscriberRegistry.CommandObservers => commandObservers;
        Dictionary<Type, List<Type>> ISubscriberRegistry.DomainEventObservers => domainEventObservers;
        Dictionary<Type, List<Type>> ISubscriberRegistry.IntegrationEventObservers => integrationEventObservers;

        public void Register<TMessage, TImplementation>() where TMessage : class, IMessage
            where TImplementation : class, IMessageHandler<TMessage>
        {
            var observed = observers.ContainsKey(typeof(TMessage));
            if (!observed)
                observers.Add(typeof(TMessage), new List<Type> { typeof(TImplementation) });
            else
                observers[typeof(TMessage)].Add(typeof(TImplementation));
        }

        public void RegisterCommand<TMessage, TImplementation>() where TMessage : class, ICommand
            where TImplementation : class, ICommandHandler<TMessage>
        {
            var observed = this.commandObservers.ContainsKey(typeof(TMessage));
            if (!observed)
                this.commandObservers.Add(typeof(TMessage), new List<Type> { typeof(TImplementation) });
            else
                this.commandObservers[typeof(TMessage)].Add(typeof(TImplementation));
        }

        public void RegisterDomainEvent<TMessage, TImplementation>() where TMessage : class, IDomainEvent
            where TImplementation : class, IDomainEventHandler<TMessage>
        {
            var observed = this.domainEventObservers.ContainsKey(typeof(TMessage));
            if (!observed)
                this.domainEventObservers.Add(typeof(TMessage), new List<Type> { typeof(TImplementation) });
            else
                this.domainEventObservers[typeof(TMessage)].Add(typeof(TImplementation));
        }

        public void RegisterIntegrationEvent<TMessage, TImplementation>() where TMessage : class, IIntegrationEvent
            where TImplementation : class, IIntegrationEventHandler<TMessage>
        {
            var observed = this.integrationEventObservers.ContainsKey(typeof(TMessage));
            if (!observed)
                this.integrationEventObservers.Add(typeof(TMessage), new List<Type> { typeof(TImplementation) });
            else
                this.integrationEventObservers[typeof(TMessage)].Add(typeof(TImplementation));
        }

        public IEnumerable<Type> Get<TMessage>() where TMessage : class, IMessage
        {
            var observed = observers.ContainsKey(typeof(TMessage));
            return observed ? observers[typeof(TMessage)] : new List<Type>();
        }

        public IEnumerable<Type> GetCommands<TMessage>() where TMessage : class, ICommand
        {
            var observed = this.commandObservers.ContainsKey(typeof(TMessage));
            return observed ? this.commandObservers[typeof(TMessage)] : new List<Type>();
        }

        public IEnumerable<Type> GetDomainEvents<TMessage>() where TMessage : class, IDomainEvent
        {
            var observed = this.domainEventObservers.ContainsKey(typeof(TMessage));
            return observed ? this.domainEventObservers[typeof(TMessage)] : new List<Type>();
        }

        public IEnumerable<Type> GetIntegrationEvents<TMessage>() where TMessage : class, IIntegrationEvent
        {
            var observed = this.integrationEventObservers.ContainsKey(typeof(TMessage));
            return observed ? this.integrationEventObservers[typeof(TMessage)] : new List<Type>();
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