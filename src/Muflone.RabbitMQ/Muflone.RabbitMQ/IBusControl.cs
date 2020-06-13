using System;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages;
using Muflone.RabbitMQ.Abstracts;
using RabbitMQ.Client;

namespace Muflone.RabbitMQ
{
    public interface IBusControl
    {
        IModel RabbitMQChannel { get; }

        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);

        Task RegisterConsumer<T>(Action<T> handler, CancellationToken cancellationToken) where T : IMessage;
    }
}