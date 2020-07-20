using System;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using RabbitMQ.Client;

namespace Muflone.RabbitMQ
{
    public interface IBusControl
    {
        IModel RabbitMQChannel { get; }

        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);

        Task RegisterCommandConsumers(Type message, CancellationToken cancellationToken = default);
        Task RegisterCommandConsumer<T>(CancellationToken cancellationToken = default)
            where T : class, ICommand;
        Task RegisterEventConsumer<T>(CancellationToken cancellationToken = default)
            where T : class, IEvent;

        Task RegisterConsumer<T>(CancellationToken cancellationToken = default) where T : class, IMessage;
    }
}