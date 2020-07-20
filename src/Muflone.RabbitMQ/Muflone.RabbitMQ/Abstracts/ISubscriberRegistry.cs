using System;
using System.Collections.Generic;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface ISubscriberRegistry
    {
        void Register<TMessage, TImplementation>() where TMessage : class, IMessage
            where TImplementation : class, IMessageHandler<TMessage>;

        void RegisterCommand<TMessage, TImplementation>() where TMessage : class, ICommand
            where TImplementation : class, ICommandHandler<TMessage>;
        void RegisterDomainEvent<TMessage, TImplementation>() where TMessage : class, IDomainEvent
            where TImplementation : class, IDomainEventHandler<TMessage>;
        void RegisterIntegrationEvent<TMessage, TImplementation>() where TMessage : class, IIntegrationEvent
            where TImplementation : class, IIntegrationEventHandler<TMessage>;

        IEnumerable<Type> Get<TMessage>() where TMessage : class, IMessage;
        IEnumerable<Type> GetCommands<TMessage>() where TMessage : class, ICommand;
        IEnumerable<Type> GetDomainEvents<TMessage>() where TMessage : class, IDomainEvent;
        IEnumerable<Type> GetIntegrationEvents<TMessage>() where TMessage : class, IIntegrationEvent;

        Dictionary<Type, List<Type>> Observers { get; }
        Dictionary<Type, List<Type>> CommandObservers { get; }
        Dictionary<Type, List<Type>> DomainEventObservers { get; }
        Dictionary<Type, List<Type>> IntegrationEventObservers { get; }
    }
}