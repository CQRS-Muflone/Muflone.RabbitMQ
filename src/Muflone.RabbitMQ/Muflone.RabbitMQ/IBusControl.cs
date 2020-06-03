using System.Threading;
using System.Threading.Tasks;
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

        Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand: class, ICommand;

        Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class, IDomainEvent;
    }
}