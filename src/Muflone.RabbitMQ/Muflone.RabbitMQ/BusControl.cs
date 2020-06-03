using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;

namespace Muflone.RabbitMQ
{
    public class BusControl : IBusControl
    {
        public IModel RabbitMQChannel { get; private set; }

        public BusControl(IOptions<BrokerProperties> options)
        {
            var connectionFactory = RabbitMqFactories.CreateConnectionFactory(options.Value);
            var connection = RabbitMqFactories.CreateConnection(connectionFactory);

            this.RabbitMQChannel = RabbitMqFactories.CreateChannel(connection);
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (!this.RabbitMQChannel.IsClosed)
                this.RabbitMQChannel.Close();

            this.RabbitMQChannel.Dispose();

            return Task.CompletedTask;
        }

        public async Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : class, ICommand
        {
            try
            {
                var messageBody = RabbitMqMappers.MapMufloneMessageToRabbitMq(command);
                this.RabbitMQChannel.BasicPublish("", typeof(TCommand).Name, null, messageBody);

                await Task.Yield();
            }
            catch (Exception ex)
            {
                throw new Exception($"Original Message: {command} - StackTrace: {ex.StackTrace}");
            }
        }

        public async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IDomainEvent
        {
            try
            {
                var messageBody = RabbitMqMappers.MapMufloneMessageToRabbitMq(@event);
                this.RabbitMQChannel.BasicPublish(typeof(TEvent).Name, "", null, messageBody);

                await Task.Yield();
            }
            catch (Exception ex)
            {
                throw new Exception($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
            }
        }
    }
}