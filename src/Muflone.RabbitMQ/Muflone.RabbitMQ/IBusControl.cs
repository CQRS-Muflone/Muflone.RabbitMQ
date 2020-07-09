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


        Task RegisterMessageConsumers(CancellationToken cancellationToken);
    }
}