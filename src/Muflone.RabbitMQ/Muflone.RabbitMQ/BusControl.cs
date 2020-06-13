using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Abstracts;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;

namespace Muflone.RabbitMQ
{
    public class BusControl : IBusControl
    {
        private readonly BrokerProperties brokerProperties;

        public IModel RabbitMQChannel { get; private set; }

        public BusControl(IOptions<BrokerProperties> options)
        {
            this.brokerProperties = options.Value;
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            var connectionFactory = RabbitMqFactories.CreateConnectionFactory(this.brokerProperties);
            var connection = RabbitMqFactories.CreateConnection(connectionFactory);

            this.RabbitMQChannel = RabbitMqFactories.CreateChannel(connection);

            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (!this.RabbitMQChannel.IsClosed)
                this.RabbitMQChannel.Close();

            this.RabbitMQChannel.Dispose();

            return Task.CompletedTask;
        }

        public async Task RegisterConsumer<T>(IMessageConsumer<T> messageConsumer, CancellationToken cancellationToken) where T : IMessage
        {
            await messageConsumer.Consume(cancellationToken);
        }
    }
}